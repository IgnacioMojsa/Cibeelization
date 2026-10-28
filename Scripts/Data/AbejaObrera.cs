public partial class AbejaObrera : Abeja{
	
	public AbejaObrera()
	{
		HP = 5;
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Agua";
		RecursosNecesarios = 2;
		RecursoGenerable = "Nectar";
	}
	
	public bool Construyendo(){
		return true;
	}
	
}
