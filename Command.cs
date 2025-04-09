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

                    // Allow the user to adjust their input based on calculated parameters
                    StairInput adjustedInput = input.GetAdjustedInput(doc, validInput, parameters);
                    if (adjustedInput != null)
                    {
                        validInput = validation.Validate(adjustedInput);
                        if (validInput == null)
                        {
                            input.ShowRetryPrompt("Invalid adjusted inputs. Values must be positive and within range.");
                            return;
                        }
                        parameters = calc.Calculate(validInput);
                        if (parameters == null)
                        {
                            ed.WriteMessage("\nCalculation failed for adjusted inputs. Check input values.");
                            return;
                        }
                        // Check for compliance issues again after adjustment
                        calc.HandleComplianceFailure(parameters);
                    }

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

                    // Check if mid-landing is needed (height > 151")
                    bool needsMidLanding = parameters.OverallHeight > 151;
                    int totalSteps = parameters.NumTreads + 1; // Treads + top landing (mid-landing replaces a tread)
                    int midStep = validInput.MidLandingAfterTread.HasValue ? validInput.MidLandingAfterTread.Value : totalSteps / 2; // Use user input or default to midpoint
                    if (needsMidLanding && (midStep <= 0 || midStep >= parameters.NumTreads))
                    {
                        midStep = totalSteps / 2; // Fallback to default if user input is invalid
                        ed.WriteMessage($"\nInvalid mid-landing position. Using default position after tread {midStep}.");
                    }
                    int treadsBeforeMid = needsMidLanding ? midStep : parameters.NumTreads;
                    int treadsAfterMid = needsMidLanding ? parameters.NumTreads - midStep : 0;
                    double treadAngleRad = parameters.TreadAngle * Math.PI / 180;
                    double midLandingAngle = Math.PI / 2; // 90° for mid-landing

                    // Create treads before mid-landing
                    IGeometry treadCreator = new Tread();
                    ed.WriteMessage("\nCreating treads before mid-landing...");
                    for (int i = 0; i < treadsBeforeMid; i++)
                    {
                        StairParameters treadParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = parameters.OverallHeight,
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = parameters.RiserHeight,
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
                        StairParameters midParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = parameters.OverallHeight,
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = parameters.RiserHeight,
                            TreadAngle = parameters.TreadAngle,
                            NumTreads = treadsBeforeMid // Position at the mid-landing step
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
                            double remainingRotation = (parameters.NumTreads - treadsBeforeMid) * treadAngleRad;
                            double newTreadAngle = remainingRotation / (parameters.NumTreads - treadsBeforeMid) * 180 / Math.PI;

                            // Calculate the height relative to the top landing
                            int treadIndexFromBottom = treadsBeforeMid + i + 1; // +1 because mid-landing replaces a tread
                            double treadHeight = treadIndexFromBottom * parameters.RiserHeight;

                            StairParameters treadParams = new StairParameters
                            {
                                CenterPoleDia = parameters.CenterPoleDia,
                                OverallHeight = parameters.OverallHeight,
                                OutsideDia = parameters.OutsideDia,
                                RotationDeg = remainingRotation * 180 / Math.PI,
                                IsClockwise = parameters.IsClockwise,
                                RiserHeight = parameters.RiserHeight,
                                TreadAngle = newTreadAngle,
                                NumTreads = treadsBeforeMid + i // Continue the tread numbering
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