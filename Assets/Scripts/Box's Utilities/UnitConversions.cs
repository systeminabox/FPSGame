
public static class UnitConversions
{
    
    /// <summary>
    /// Unity units are in meters.
    /// </summary>
    /// <param name="feet"></param>
    /// <returns></returns>
    public static float FeetToMeters(this float feet) => feet * 0.3048f;
    /// <summary>
    /// Unity units are in meters.
    /// </summary>
    /// <param name="meters"></param>
    /// <returns></returns>
    public static float MetersToFeet(this float meters) => meters * 3.28084f;
    public static float MetersToKilometers(this float meters) => meters * 0.001f;
    public static float KilometersToMeters(this float kilometers) => kilometers * 1000f;
}
