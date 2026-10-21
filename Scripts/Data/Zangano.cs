
using System.Collections.Generic;

public partial class Zangano : Abeja{
	public Zangano()
    {
        PotenciaAtaque = 1;
        RecursoGenerable = "Agua";
        TransformacionesDisponibles = new List<Abeja>(){new AbejaObrera(), new AbejaPolinizadora()};
    }
}
