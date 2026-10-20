public partial class AbejaObrera : Abeja{
	
	public AbejaObrera()
	{
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "agua";
		RecursosNecesarios = 2;
	}
	
	public bool Construyendo(){
		return true;
	}
	
}
