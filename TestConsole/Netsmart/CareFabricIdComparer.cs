using Netsmart.Bedrock.CareFabric.Cdm.Entities;

internal class CareFabricIdComparer : IEqualityComparer<CareFabricID>
{
    public bool Equals(CareFabricID? x, CareFabricID? y)
    {
        if (x == null && y == null)
            return true;
        else if (x == null || y == null)
            return false;

        return x.ScopeID == y.ScopeID && x.ID == y.ID;
    }

    public int GetHashCode([DisallowNull] CareFabricID obj)
    {
        return HashCode.Combine(obj.ScopeID, obj.ID);
    }
}
