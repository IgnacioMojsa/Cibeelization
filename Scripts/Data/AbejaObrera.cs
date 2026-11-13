using Microsoft.CodeAnalysis.CSharp.Syntax;

public partial class AbejaObrera : Abeja{
	
	public bool ModoDestruccion {get; set;} = false;
	
	public AbejaObrera()
	{
		HP = 5;
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Agua";
		RecursosNecesarios = 2;
		RecursoGenerable = "Propoleo";
	}
	
	override public void HabilidadDeAbeja()
	{
		HabilidadActiva = true;
		ModoDestruccion = true;
	}
	
}
