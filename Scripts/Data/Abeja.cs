using System;
using System.Collections.Generic;
using System.Linq;

public partial class Abeja{
	public int HP { get; set; } = 15;
	public string RecursoDeTransformacion { get ; set; } 
	public int RecursosNecesarios {get; set;}
	public string RecursoGenerable {get; set;}
	public bool FueraDeJuego {get; set;} = false;
	public int PotenciaAtaque {get; set;}
	public List<Abeja> TransformacionesDisponibles {get; set;} 
	public Colmena ColmenaHogar {get; set;}
	public Celda CeldaActual { get; set; }

	public bool AptaParaTransformar(Colmena unaColmena){
		var tieneLosRecursosNecesarios = TransformacionesDisponibles.Any(a => unaColmena.CantidadTotalDeUnRecurso(a.RecursoDeTransformacion) >= RecursosNecesarios);
		var tieneElRecursoNecesario = TransformacionesDisponibles.Any(a => unaColmena.TieneRecurso(a.RecursoDeTransformacion));

		return tieneElRecursoNecesario && tieneLosRecursosNecesarios;
	}

	public void RestarVida(Abeja atacante)
	{
		if(atacante is AbejaReina reina && reina.AtaquePotenciado)
		{
			HP -= atacante.PotenciaAtaque * 2;
		}
		else
		{
			HP -= atacante.PotenciaAtaque;
		}
	}

	public void Curar()
	{
		HP = Math.Min(15, HP + 5);
	}

	public void MatarAbeja(){
		HP -= HP;
		FueraDeJuego = true;
	}
}
