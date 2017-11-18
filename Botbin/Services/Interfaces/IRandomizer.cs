namespace Botbin.Services.Interfaces {
    public interface IRandomizer {
        int Integer();
        int Integer(int max);
        int Integer(int min, int max);
    }
}