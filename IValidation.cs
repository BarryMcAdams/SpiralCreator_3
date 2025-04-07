namespace SpiralStairPlugin
{
    public interface IValidation
    {
        ValidatedStairInput Validate(StairInput input);
    }
}