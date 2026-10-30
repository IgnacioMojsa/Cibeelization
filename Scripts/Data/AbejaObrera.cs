public partial class AbejaObrera : Abeja{
	
	public AbejaObrera()
	{
		HP = 5;
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Agua";
		RecursosNecesarios = 2;
		RecursoGenerable = "Propoleo";
	}
	
	public bool Construyendo(){
		return true;
	}
	
}
