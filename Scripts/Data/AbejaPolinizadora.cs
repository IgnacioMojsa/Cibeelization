using System.Collections.Generic;

public partial class AbejaPolinizadora : Abeja{
	public AbejaPolinizadora()
    {
        PotenciaAtaque = 2;
		RecursoDeTransformacion = "Nectar";
		RecursosNecesarios = 3;
		RecursoGenerable = "Polen";
    }
}