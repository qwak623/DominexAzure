namespace GameCore.GameCore;
public class TempEffects
{
	public int GeneralCostReduction { get; private set; } = 0;
	public void ReduceCost(int amount)
	{
		GeneralCostReduction += amount;
	}

	public void Reset()
	{
		GeneralCostReduction = 0;
	}
}
