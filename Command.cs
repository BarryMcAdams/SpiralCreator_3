using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;

namespace SpiralStairPlugin
{
    public class Command
    {
        [CommandMethod("CREATESPIRAL")]
        public void CreateSpiralStaircase()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
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

                    EntityCollection entities = new EntityCollection();
                    // Create all treads except the top one
                    IGeometry[] treadCreators = new IGeometry[] { new CenterPole(), new Tread() };
                    ed.WriteMessage("\nStarting geometry creation...");
                    for (int i = 0; i < treadCreators.Length; i++)
                    {
                        ed.WriteMessage($"\nCreating geometry {i + 1} of {treadCreators.Length + 1}...");
                        Entity[] geometry = treadCreators[i].Create(doc, parameters);
                        ed.WriteMessage($"\nGeometry {i + 1} created with {geometry.Length} entities.");
                        for (int j = 0; j < geometry.Length; j++)
                        {
                            entities.Add(geometry[j]);
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