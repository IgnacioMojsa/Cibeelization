using Godot;
using System;

public partial class AbejaReina : Abeja
{
	public bool EsSuTurno {get; set;} = false;
	public bool TiroLosDados {get; set;} = false;
	public bool SeMovio {get; set;} = false;

	public bool AtacoRecien {get; set;} = false;
	public int MovimientosDisponibles {get; set;}
	public int Id {get; set;}

	public Celda UbicacionActual {get; set;}

	public EstadoTurno Estado {get; set;} = EstadoTurno.EsperandoDado;
 
	public enum EstadoTurno 
	{
		EsperandoDado,
		EsperandoAccion,
		TurnoTerminado
	} 

	public AbejaReina(int id){
		Id = id;
	}

	/*
	public void Mover(Vector3 nuevaPosicion)
	{
		GlobalPosition = nuevaPosicion;
	}
	*/
}
