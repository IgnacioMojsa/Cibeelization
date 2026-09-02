
using Godot;
using System.Collections.Generic;

public partial class Colmena
{
	List<string> RecursosDeColmena = new List<string>();
	List<Abeja> AbejasDeColmena = new List<Abeja>();

	public bool TieneRecurso(string unRecurso){
		return RecursosDeColmena.Contains(unRecurso);
	}

	public void CanjearRecurso(string unRecurso){
		if(TieneRecurso("JaleaReal")){
			RecursosDeColmena.Add(unRecurso);
		}
	}
}
