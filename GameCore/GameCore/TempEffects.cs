
namespace GameCore.GameCore;
public class TempEffects
{
	public int GeneralCostReduction { get; private set; } = 0;
	public int CopperValueIncrease { get; private set; } = 0;
	public int FirstSilverValueIncrease { get; private set; } = 0;

	public void ReduceCost(int amount)
	{
		GeneralCostReduction += amount;
	}

	public void Reset()
	{
		GeneralCostReduction = 0;
		CopperValueIncrease = 0;
		FirstSilverValueIncrease = 0;
	}

	public void IncreaseCopperValue(int amount)
	{
		CopperValueIncrease += amount;
	}

	public void IncreaseFirstSilverValue()
	{
		FirstSilverValueIncrease++;
	}

	public void ClearFirstSilverValue()
	{
		FirstSilverValueIncrease = 0;
	}
}
