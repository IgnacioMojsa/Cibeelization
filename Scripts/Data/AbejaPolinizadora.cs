using System.Collections.Generic;

public partial class AbejaPolinizadora : Abeja{
	public AbejaPolinizadora()
    {
        HP = 3;
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Propoleo";
		RecursosNecesarios = 3;
		RecursoGenerable = "Polen";
    }

	override public void HabilidadDeAbeja()
	{
		var movimientoMejorado = ColmenaHogar.ReinaDeColmena.MovimientosDisponibles * 2;

		ColmenaHogar.ReinaDeColmena.MovimientosDisponibles = movimientoMejorado;
	} 
}