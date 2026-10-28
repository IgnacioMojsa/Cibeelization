using System.Collections.Generic;

public partial class AbejaPolinizadora : Abeja{
	public AbejaPolinizadora()
    {
        HP = 3;
		PotenciaAtaque = 2;
		RecursoDeTransformacion = "Nectar";
		RecursosNecesarios = 3;
		RecursoGenerable = "Polen";
    }
}