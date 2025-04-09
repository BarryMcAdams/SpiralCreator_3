using Autodesk.AutoCAD.ApplicationServices;

namespace SpiralStairPlugin
{
    public interface IInput
    {
        StairInput GetInput(Document doc);
        StairInput GetAdjustedInput(Document doc, ValidatedStairInput validInput, StairParameters parameters);
        void ShowRetryPrompt(string message);
    }
}