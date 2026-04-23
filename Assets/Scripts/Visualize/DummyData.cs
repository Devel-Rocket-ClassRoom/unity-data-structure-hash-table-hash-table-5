using System.Collections.Generic;

// "apple", "cheery"가 충돌났다고 가정
public struct DummySimple
{
    public HashTableSnapshot<string, int> GetSnapshot()
    {
        var slots = new HashSlot<string, int>[7];

        slots[0] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[1] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[2] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "banana",
            Value = 2
        };
        slots[3] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "apple",
            Value = 1
        };
        slots[4] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "cherry",
            Value = 3
        };
        slots[5] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[6] = new HashSlot<string, int> { State = SlotState.Empty };

        return new HashTableSnapshot<string, int>
        {
            TableType = "Simple",
            Capacity = slots.Length,
            Count = 3,
            Slots = slots
        };
    }
}

public struct DummyChain
{
    public HashTableSnapshot<string, int> GetSnapshot()
    {
        var slots = new HashSlot<string, int>[7];

        slots[0] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[1] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "apple",
            Value = 5,
            Chain = new List<(string, int)> { ("apple", 5), ("cheery", 6) }
        };
        slots[2] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[3] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "banana",
            Value = 3,
            Chain = new List<(string, int)> { ("banana", 3) }
        };
        slots[4] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[5] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[6] = new HashSlot<string, int> { State = SlotState.Empty };

        return new HashTableSnapshot<string, int>
        {
            TableType = "Chaining",
            Capacity = slots.Length,
            Count = 3,
            Slots = slots
        };
    }
}

public struct DummyOpenAddressing
{
    public HashTableSnapshot<string, int> GetSnapshot()
    {
        var slots = new HashSlot<string, int>[7];

        slots[0] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[1] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "apple",
            Value = 5,
        };
        slots[2] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[3] = new HashSlot<string, int>
        {
            State = SlotState.Occupied,
            Key = "banana",
            Value = 3,
        };
        slots[4] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[5] = new HashSlot<string, int> { State = SlotState.Empty };
        slots[6] = new HashSlot<string, int> { State = SlotState.Empty };

        return new HashTableSnapshot<string, int>
        {
            TableType = "OpenAddressing",
            Capacity = slots.Length,
            Count = 2,
            Slots = slots,
            Strategy = ProbingStrategy.Linear
        };
    }
}
