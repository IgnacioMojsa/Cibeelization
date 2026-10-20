using System.Collections.Generic;
using System.Linq;

public partial class Colmena
{
	public List<string> RecursosDeColmena = new List<string>();
	public List<Abeja> AbejasDeColmena = new List<Abeja>();
	public AbejaReina ReinaDeColmena {get; set;}

	public Colmena(AbejaReina reina){
		ReinaDeColmena = reina;
	}

	public List<Celda> CeldasEnPropiedad = new List<Celda>();

	public Abeja EncontrarAbeja(Abeja unaAbeja){
		var abejaABuscar = AbejasDeColmena.Find(a => a == unaAbeja);
		
		return abejaABuscar;
	}
	
	public bool TieneRecurso(string unRecurso){
		return RecursosDeColmena.Contains(unRecurso);
	}

	public int CantidadTotalDeUnRecurso(string unRecurso)
	{
		return RecursosDeColmena.Where(r => r == unRecurso).ToList().Count;
	}

	public void CanjearRecurso(string unRecurso){
		if(TieneRecurso("JaleaReal")){
			RecursosDeColmena.Add(unRecurso);
		}
	}
}
