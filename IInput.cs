using Autodesk.AutoCAD.ApplicationServices;

namespace SpiralStairPlugin
{
    public interface IInput
    {
        StairInput GetInput(Document doc);
        void ShowRetryPrompt(string errorMessage);
        StairInput GetAdjustedInput(Document doc, StairParameters parameters);
    }
}