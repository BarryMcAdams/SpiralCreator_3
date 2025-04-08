using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Windows.Forms; // For DialogResult

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
                    StairInput userInput;
                    ValidatedStairInput validInput;
                    StairParameters parameters;

                    while (true)
                    {
                        userInput = input.GetInput(doc);
                        if (userInput == null)
                        {
                            ed.WriteMessage("\nCommand canceled.");
                            return;
                        }

                        IValidation validation = new Validation();
                        validInput = validation.Validate(userInput);
                        if (validInput == null)
                        {
                            input.ShowRetryPrompt("Invalid inputs. Values must be positive and within range.");
                            continue;
                        }

                        ICalc calc = new Calc();
                        parameters = calc.Calculate(validInput);
                        if (parameters == null)
                        {
                            ed.WriteMessage("\nCalculation failed. Check input values.");
                            return;
                        }

                        // Check compliance with IBC R311.7.10.1
                        ComplianceChecker checker = new ComplianceChecker();
                        var violations = checker.CheckCompliance(parameters);
                        if (violations.Count > 0)
                        {
                            using (ComplianceViolationForm form = new ComplianceViolationForm(violations))
                            {
                                DialogResult result = form.ShowDialog();
                                switch (form.UserChoice)
                                {
                                    case ComplianceViolationForm.Result.TryAgain:
                                        continue; // Loop back to get new inputs
                                    case ComplianceViolationForm.Result.Ignore:
                                        break; // Proceed despite violations
                                    case ComplianceViolationForm.Result.Cancel:
                                        ed.WriteMessage("\nCommand canceled due to compliance violations.");
                                        return;
                                }
                            }
                        }
                        break; // No violations or user chose to ignore
                    }

                    EntityCollection entities = new EntityCollection();
                    // Create center pole
                    IGeometry centerPole = new CenterPole();
                    ed.WriteMessage("\nCreating center pole...");
                    Entity[] poleGeometry = centerPole.Create(doc, parameters);
                    for (int j = 0; j < poleGeometry.Length; j++)
                    {
                        entities.Add(poleGeometry[j]);
                    }

                    // Check if mid-landing is needed (height > 151")
                    bool needsMidLanding = parameters.OverallHeight > 151;
                    int totalSteps = parameters.NumTreads + 1; // Treads + top landing
                    int midStep = totalSteps / 2; // Midpoint for mid-landing
                    int treadsBeforeMid = needsMidLanding ? midStep : parameters.NumTreads;
                    int treadsAfterMid = needsMidLanding ? parameters.NumTreads - midStep : 0;

                    // Create treads before mid-landing
                    IGeometry treadCreator = new Tread();
                    ed.WriteMessage("\nCreating treads before mid-landing...");
                    for (int i = 0; i < treadsBeforeMid; i++)
                    {
                        // Create a temporary parameters object for this tread
                        StairParameters treadParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = parameters.OverallHeight,
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = parameters.RiserHeight,
                            TreadAngle = parameters.TreadAngle,
                            NumTreads = i // Set the tread index
                        };
                        Entity[] treadGeometry = treadCreator.Create(doc, treadParams);
                        for (int j = 0; j < treadGeometry.Length; j++)
                        {
                            entities.Add(treadGeometry[j]);
                        }
                    }

                    // Create mid-landing if needed
                    if (needsMidLanding)
                    {
                        ed.WriteMessage("\nCreating mid-landing...");
                        IGeometry midLanding = new MidLanding();
                        // Use a temporary parameters object for mid-landing
                        StairParameters midParams = new StairParameters
                        {
                            CenterPoleDia = parameters.CenterPoleDia,
                            OverallHeight = parameters.OverallHeight,
                            OutsideDia = parameters.OutsideDia,
                            RotationDeg = parameters.RotationDeg,
                            IsClockwise = parameters.IsClockwise,
                            RiserHeight = parameters.RiserHeight,
                            TreadAngle = parameters.TreadAngle,
                            NumTreads = midStep
                        };
                        Entity[] midGeometry = midLanding.Create(doc, midParams);
                        for (int j = 0; j < midGeometry.Length; j++)
                        {
                            entities.Add(midGeometry[j]);
                        }

                        // Create treads after mid-landing
                        ed.WriteMessage("\nCreating treads after mid-landing...");
                        for (int i = midStep; i < parameters.NumTreads; i++)
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
                            Entity[] treadGeometry = treadCreator.Create(doc, treadParams);
                            for (int j = 0; j < treadGeometry.Length; j++)
                            {
                                entities.Add(treadGeometry[j]);
                            }
                        }
                    }

                    // Add the top landing as the final "tread"
                    ed.WriteMessage("\nCreating top landing...");
                    IGeometry topLanding = new TopLanding();
                    Entity[] landingGeometry = topLanding.Create(doc, parameters);
                    ed.WriteMessage($"\nTop landing created with {landingGeometry.Length} entities.");
                    for (int j = 0; j < landingGeometry.Length; j++)
                    {
                        entities.Add(landingGeometry[j]);
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