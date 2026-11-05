using System.Collections.Generic;

public partial class AbejaConstructora : Abeja{
	public AbejaConstructora()
    {
        HP = 2;
		PotenciaAtaque = 1;
		RecursoDeTransformacion = "Agua";
		RecursosNecesarios = 6;
		RecursoGenerable = "Propoleo";
    }

	override public void HabilidadDeAbeja()
	{
		HabilidadActiva = true;
	} 
}