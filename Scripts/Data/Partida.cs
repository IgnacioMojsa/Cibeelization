namespace JuegoAbeja.Scripts.Data
{
    public class Partida
    {
        public int CantidadJugadores { get; set; }
        public int SizeTablero { get; set; }

        public Partida(int cantidadJugadores, int sizeTablero)
        {
            CantidadJugadores = cantidadJugadores;
            SizeTablero = sizeTablero;
        }
    }
}
