public partial class AbejaObrera : Abeja{
	
	public AbejaObrera()
	{
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Agua";
		RecursosNecesarios = 2;
		RecursoGenerable = "Nectar";
	}
	
	public bool Construyendo(){
		return true;
	}
	
}
