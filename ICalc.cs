namespace SpiralStairPlugin
{
    public interface ICalc
    {
        StairParameters Calculate(ValidatedStairInput input);
        ComplianceRetryOption HandleComplianceFailure(StairParameters parameters);
    }

    public class ComplianceRetryOption
    {
        public bool ShouldRetry { get; set; }
        public string Message { get; set; }
    }
}