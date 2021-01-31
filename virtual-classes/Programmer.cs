using System;

public class Programmer : Creator
{
    public override void Create()
    {
        throw new NotImplementedException();
    }

    private void BrainStirnSolutions()
    {
        base.CreativityLevel = base.CreativityLevel - 15;
        base.Energy = base.Energy - 10;
        Console.WriteLine("Brainstorming solutions...");
    }

    private void PickMostOptimalSolution()
    {
        base.CreativityLevel = base.CreativityLevel - 5;
        base.Energy = base.Energy - 5;
        Console.WriteLine("Picking most optimal solution...");
    }

    private void WriteCode()
    {
        base.CreativityLevel = base.CreativityLevel - 7;
        base.Energy = base.Energy - 5;
        Console.WriteLine("Writing code...");
    }

    private void TestCode()
    {
        base.CreativityLevel = base.CreativityLevel - 3;
        base.Energy = base.Energy - 3;
        Console.WriteLine("Testing code...");
    }
}