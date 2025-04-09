namespace SpiralStairPlugin
{
    public interface ICalc
    {
        StairParameters Calculate(ValidatedStairInput input);
        void HandleComplianceFailure(StairParameters parameters);
    }
}