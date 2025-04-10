using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;

namespace SpiralStairPlugin
{
    public class Command
    {
        [CommandMethod("CREATESPIRAL")]
        public void CreateSpiralStaircase()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    IInit init = new Init();
                    AutoCADContext context = init.Initialize();

                    IInput input = new Input();
                    StairInput userInput = input.GetInput(doc);
                    if (userInput == null)
                    {
                        ed.WriteMessage("\nCommand canceled.");
                        return;
                    }

                    IValidation validation = new Validation();
                    ValidatedStairInput validInput = validation.Validate(userInput);
                    if (validInput == null)
                    {
                        input.ShowRetryPrompt("Invalid inputs. Values must be positive and within range.");
                        return;
                    }

                    ICalc calc = new Calc();
                    StairParameters parameters = calc.Calculate(validInput);
                    if (parameters == null)
                    {
                        ed.WriteMessage("\nCalculation failed. Check input values.");
                        return;
                    }

                    // Check for compliance issues
                    calc.HandleComplianceFailure(parameters);

                    EntityCollection entities = new EntityCollection();
                    // Create center pole
                    IGeometry centerPole = new CenterPole();
                    ed.WriteMessage("\nCreating center pole...");
                    Entity[] poleGeometry = centerPole.Create(doc, tr, parameters);
                    for (int j = 0; j < poleGeometry.Length; j++)
                    {
                        if (poleGeometry[j] != null)
                        {
                            entities.Add("CenterPole", poleGeometry[j]);
                        }
                        else
                        {
                            ed.WriteMessage($"\nWarning: poleGeometry[{j}] is null and will be skipped.");
                        }
                    }

                    // Check if mid-landing is needed
                    bool needsMidLanding = validInput.MidLandingAfterTread.HasValue;
                    int totalTreads = parameters.NumTreads; // Total treads excluding landings
                    int midStep = validInput.MidLandingAfterTread ?? 0; // Use user input if provided
                    int treadsBeforeMid = needsMidLanding ? midStep : totalTreads;
                    int treadsAfterMid = needsMidLanding ? totalTreads - midStep : 0;
                    double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                    double midLandingAngle = Math.PI / 2; // 90° for mid-landing
                    double treadThickness = 0.25; // Assuming tread thickness is 0.25"
                    double landingThickness = 0.25; // Assuming mid-landing thickness is 0.25"
                    double desiredGap = 8.5; // Gap between last tread's top face and top landing's bottom face

                    // Calculate riser height to ensure the last tread's top face is 8.5" below the top landing
                    double topLandingZ = parameters.OverallHeight; // Top landing's bottom face
                    double lastTreadTopZ = topLandingZ - desiredGap; // Last tread's top face
                    double lastTreadBottomZ = lastTreadTopZ - treadThickness; // Last tread's bottom face
                    double effectiveHeight = lastTreadBottomZ; // Height up to the last tread's bottom face
                    double riserHeight = effectiveHeight / totalTreads; // Total treads, not total steps, to account for landings

                    // Update parameters with the corrected riser height
                    parameters.RiserHeight = riserHeight;

                    // Create treads before mid-landing
                    IGeometry treadCreator = new Tread();
                    ed.WriteMessage("\nCreating treads before mid-landing...");
                    for (int i = 0; i < treadsBeforeMid; i++)
                    {
                        // Calculate Z-position from the bottom up
                        double treadBottomZ = i * riserHeight;
                        StairParameters treadParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = treadBottomZ, // Use Z-position for this tread
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = riserHeight,
                            TreadAngle = parameters.TreadAngle,
                            NumTreads = i
                        };
                        Entity[] treadGeometry = treadCreator.Create(doc, tr, treadParams);
                        for (int j = 0; j < treadGeometry.Length; j++)
                        {
                            if (treadGeometry[j] != null)
                            {
                                entities.Add("Tread", treadGeometry[j]);
                            }
                            else
                            {
                                ed.WriteMessage($"\nWarning: treadGeometry[{j}] (tread {i}) is null and will be skipped.");
                            }
                        }
                    }

                    // Create mid-landing if needed (replaces the tread at midStep)
                    if (needsMidLanding)
                    {
                        ed.WriteMessage("\nCreating mid-landing...");
                        IGeometry midLanding = new MidLanding();
                        double midLandingZ = treadsBeforeMid * riserHeight; // Position at the mid-landing step
                        StairParameters midParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = midLandingZ,
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = riserHeight,
                            TreadAngle = parameters.TreadAngle,
                            NumTreads = treadsBeforeMid
                        };
                        Entity[] midGeometry = midLanding.Create(doc, tr, midParams);
                        for (int j = 0; j < midGeometry.Length; j++)
                        {
                            if (midGeometry[j] != null)
                            {
                                entities.Add("MidLanding", midGeometry[j]);
                            }
                            else
                            {
                                ed.WriteMessage($"\nWarning: midGeometry[{j}] is null and will be skipped.");
                            }
                        }

                        // Create treads after mid-landing, adjusting for mid-landing's 90° span
                        ed.WriteMessage("\nCreating treads after mid-landing...");
                        for (int i = 0; i < treadsAfterMid; i++)
                        {
                            double angleOffset = midLandingAngle * (parameters.IsClockwise ? 1 : -1);
                            double adjustedStartAngle = (treadsBeforeMid * treadAngleRad + angleOffset) * (parameters.IsClockwise ? 1 : -1);
                            double remainingRotation = (totalTreads - treadsBeforeMid) * treadAngleRad;
                            double newTreadAngle = remainingRotation / (totalTreads - treadsBeforeMid) * 180 / Math.PI;

                            // Calculate the height relative to the top landing
                            int treadIndexFromBottom = treadsBeforeMid + i; // No +1 because mid-landing replaces a tread
                            double treadBottomZ = treadIndexFromBottom * riserHeight + landingThickness;

                            // Adjust the last tread to ensure the gap is exactly 8.5"
                            if (i == treadsAfterMid - 1)
                            {
                                treadBottomZ = lastTreadBottomZ;
                            }

                            StairParameters treadParams = new StairParameters
                            {
                                CenterPoleDia = parameters.CenterPoleDia,
                                OverallHeight = treadBottomZ,
                                OutsideDia = parameters.OutsideDia,
                                RotationDeg = remainingRotation * 180 / Math.PI,
                                IsClockwise = parameters.IsClockwise,
                                RiserHeight = riserHeight,
                                TreadAngle = newTreadAngle,
                                NumTreads = treadsBeforeMid + i
                            };
                            Entity[] treadGeometry = treadCreator.Create(doc, tr, treadParams);
                            for (int j = 0; j < treadGeometry.Length; j++)
                            {
                                if (treadGeometry[j] != null)
                                {
                                    // Adjust the tread's rotation to account for the mid-landing's 90°
                                    treadGeometry[j].TransformBy(Matrix3d.Rotation(angleOffset, Vector3d.ZAxis, Point3d.Origin));
                                    entities.Add("Tread", treadGeometry[j]);
                                }
                                else
                                {
                                    ed.WriteMessage($"\nWarning: treadGeometry[{j}] (tread {i}) is null and will be skipped.");
                                }
                            }
                        }
                    }

                    // Add the top landing as the final "tread"
                    ed.WriteMessage("\nCreating top landing...");
                    IGeometry topLanding = new TopLanding();
                    Entity[] landingGeometry = topLanding.Create(doc, tr, parameters);
                    for (int j = 0; j < landingGeometry.Length; j++)
                    {
                        if (landingGeometry[j] != null)
                        {
                            entities.Add("TopLanding", landingGeometry[j]);
                        }
                        else
                        {
                            ed.WriteMessage($"\nWarning: landingGeometry[{j}] is null and will be skipped.");
                        }
                    }
                    ed.WriteMessage("\nAll geometry created.");

                    ITweaks tweaks = new Tweaks();
                    entities = tweaks.ApplyTweaks(doc, entities, validInput, parameters);

                    IOutput output = new Output();
                    output.Finalize(doc, validInput, parameters, entities);

                    tr.Commit();
                    ed.WriteMessage("\nSpiral staircase created successfully!");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage($"\nError: {ex.Message}");
                    tr.Abort();
                }
            }
        }
    }
}