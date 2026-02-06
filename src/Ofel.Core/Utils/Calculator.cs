namespace Ofel.Core.Utils
{
    internal class Calculator // ou public si tu veux exposer
    {
        public static double ToRadians(double angleInDegrees)
        {
            return (Math.PI / 180) * angleInDegrees;
        }
    }
}
