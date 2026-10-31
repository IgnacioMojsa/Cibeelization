using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TroopsManager : Node3D
{
    [Export] private Tablero tableroActual;
	[Export] private PlayerManager playerManager;
	[Export] public CeldasManager celdasManager;
	private readonly Dictionary<Type, PackedScene> Assets = new();
	public Dictionary<Abeja, Node3D> VisualAbejas = new();
	public Dictionary<AbejaReina, Node3D> AlmacenJugadores = new();
	public List<Abeja> AbejasCercanas = new();
	public List<Abeja> AbejasTransformables = new List<Abeja>();
	public Node3D VisualSubditoActual;
	public Abeja SubditoActual;
    public override void _Ready()
	{
	    AlmacenJugadores.Clear();
	    CargarAssets();
	    if (GameManager.Instance.JugadoresEnPartida.Count > 0)
	        CargarAlmacenDeTropasPara(GameManager.Instance.cantidadJugadores);
	}

	public void CargarAssets()
	{
		Assets.Clear();
	
	    Assets.Add(typeof(Zangano), GD.Load<PackedScene>("res://Scenes/Zangano.tscn"));
		Assets.Add(typeof(AbejaObrera), GD.Load<PackedScene>("res://Scenes/AbejaObrera.tscn"));
		Assets.Add(typeof(AbejaPolinizadora), GD.Load<PackedScene>("res://Scenes/AbejaPolinizadora.tscn"));
		Assets.Add(typeof(AbejaGuardiana), GD.Load<PackedScene>("res://Scenes/AbejaGuardiana.tscn"));
	}

    public void InstanciarAbeja(Celda celdaCliqueada)
	{
	    int indiceCelda = tableroActual.Celdas.IndexOf(celdaCliqueada);
		if(GameManager.Instance.modoMultiplayer){
			Rpc(nameof(InstanciarAbejaRpc), indiceCelda);
		} else {
			InstanciarAbejaLocal(celdaCliqueada);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void InstanciarAbejaRpc(int indiceCelda){
		if(indiceCelda < 0 || indiceCelda >= tableroActual.Celdas.Count) return;
		Celda celdaCliqueada = tableroActual.Celdas[indiceCelda];
		InstanciarAbejaLocal(celdaCliqueada);
	}

	private void InstanciarAbejaLocal(Celda celdaCliqueada){
		if (Assets.Count == 0)
	    {
	        GD.PrintErr("No hay assets cargados para invocar abejas");
	        return;
	    }

	    if (!AlmacenJugadores.ContainsKey(GameManager.Instance.jugadorEnTurno))
	    {
	        GD.PrintErr("No existe almacén de tropas para este jugador");
	        return;
	    }

	    var abejaNueva = new Zangano();
	    var InstanciaNueva = Assets[typeof(Zangano)].Instantiate<Node3D>();

	    abejaNueva.ColmenaHogar = GameManager.Instance.jugadorEnTurno.ColmenaDeReina;
	    abejaNueva.CeldaActual = celdaCliqueada;
	    GameManager.Instance.GenerarAbejaNueva(abejaNueva);

	    VisualAbejas.Add(abejaNueva, InstanciaNueva);
		AlmacenJugadores[GameManager.Instance.jugadorEnTurno].AddChild(InstanciaNueva);
	    EstablecerPosicionDeAbeja(InstanciaNueva, celdaCliqueada.Tile.GlobalPosition);

		celdasManager.PintarCeldaDeAbeja(celdaCliqueada, abejaNueva);

	    GD.Print("Abeja instanciada en " + celdaCliqueada);
	}

	public void EstablecerPosicionDeAbeja(Node3D instanciaNueva, Vector3 posicion){
		instanciaNueva.GlobalPosition = posicion;
    }

	public void CargarAlmacenDeTropasPara(int cantidadJugadores){
		for (int j = 0; j < cantidadJugadores; j++)
		{
			var almacenJugador = new Node3D();
			almacenJugador.Name = "AlmacenJugador" + GameManager.Instance.JugadoresEnPartida[j].Id;

			AlmacenJugadores.Add(GameManager.Instance.JugadoresEnPartida[j], almacenJugador);

			AddChild(almacenJugador);
		}
	}

	public void AbsorberSubdito(Celda unaCelda)
	{
		var abejaSacrificio = GameManager.Instance.jugadorEnTurno.ColmenaDeReina.AbejasDeColmena.Find(a => a.CeldaActual == unaCelda);
		if(abejaSacrificio == null) return;

		int indexAbeja = GameManager.Instance.jugadorEnTurno.ColmenaDeReina.AbejasDeColmena.IndexOf(abejaSacrificio);
		int indexCelda = tableroActual.Celdas.IndexOf(unaCelda);

		if(GameManager.Instance.modoMultiplayer){
			Rpc(nameof(AbsorberSubditoRpc), indexAbeja, indexCelda);
		} else {
			AbsorberSubditoLocal(abejaSacrificio, unaCelda);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void AbsorberSubditoRpc(int indexAbeja, int indexCelda){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if(indexAbeja < 0 || indexAbeja >= jugador.ColmenaDeReina.AbejasDeColmena.Count) return;
		if(indexCelda < 0 || indexCelda >= tableroActual.Celdas.Count) return;

		var abejaSacrificio = jugador.ColmenaDeReina.AbejasDeColmena[indexAbeja];
		var unaCelda = tableroActual.Celdas[indexCelda];
		
		AbsorberSubditoLocal(abejaSacrificio, unaCelda);
	}

	private void AbsorberSubditoLocal(Abeja abejaSacrificio, Celda unaCelda){
		celdasManager.DespintarCeldaDeAbeja(unaCelda);
		GameManager.Instance.ConsumirAbsorcion(abejaSacrificio);
		AbejasCercanas.Remove(abejaSacrificio);

		if(VisualAbejas.TryGetValue(abejaSacrificio, out var visual) && IsInstanceValid(visual)){
			visual.QueueFree();
		}

		VisualAbejas.Remove(abejaSacrificio);
		GD.Print("El jugador " + GameManager.Instance.jugadorEnTurno.Id + " tiene " + GameManager.Instance.jugadorEnTurno.HP + " puntos de vida");
	}

	public void MostrarAbejasCercanas()
	{
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");
		
		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.CeldaActual);

		AbejasCercanas = reinaActual.ColmenaDeReina.AbejasDeColmena.Where(a => celdasAdyacentes.Contains(a.CeldaActual)).ToList();

		foreach (var abeja in AbejasCercanas)
		{
			playerManager.vfxManager.EstablecerNextPass(VisualAbejas[abeja], materialAtaque);

			//GD.Print(VisualAbejas[abeja]);
		} 
	}

	public void OcultarAbejasCercanas()
	{
		var materialOriginal = GD.Load<StandardMaterial3D>("res://outlineBase.tres");

		foreach (var abeja in AbejasCercanas)
		{
			if(abeja.FueraDeJuego) continue;
			if(!VisualAbejas.TryGetValue(abeja, out var visual) || !IsInstanceValid(visual)) continue;

			playerManager.vfxManager.EstablecerNextPass(VisualAbejas[abeja], materialOriginal);

			//GD.Print(VisualAbejas[abeja]);
		} 

		AbejasCercanas.Clear();
	}

	public bool PuedeAbsorberSubdito()
	{
		if (tableroActual == null) return false;
		
		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(GameManager.Instance.jugadorEnTurno.CeldaActual);

		if (celdasAdyacentes == null || celdasAdyacentes.Any(c => c == null)) return false;

		return celdasAdyacentes.Any(c => reinaActual.ColmenaDeReina.AbejasDeColmena.Any(abeja => abeja.CeldaActual == c)) && reinaActual.HP < 15;
	}

	public void IntentarMoverSubdito(Celda unaCelda){
		var jugador = GameManager.Instance.jugadorEnTurno;

		if (jugador.MovimientosDisponibles <= 0) return;
		
		if (!playerManager.movimientoManager.CeldasSonAdyacentes(SubditoActual.CeldaActual, unaCelda)) return;
    	if (playerManager.movimientoManager.CeldaTieneOtraAbeja(unaCelda)) return;
    	if (GameManager.Instance.JugadoresEnPartida.Any(j => !j.FueraDeJuego && j.CeldaActual == unaCelda)) return;

		int indexSubdito = jugador.ColmenaDeReina.AbejasDeColmena.IndexOf(SubditoActual);
		int indexCelda = tableroActual.Celdas.IndexOf(unaCelda);

		if(GameManager.Instance.modoMultiplayer){
			Rpc(nameof(MoverSubditoRpc), indexSubdito, indexCelda);
		} else {
			MoverSubdito(unaCelda);
		}
		
		GameManager.Instance.CamaraActual.EnfocarNodo(VisualSubditoActual, 2, 1);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void MoverSubditoRpc(int indexSubdito, int indexCelda){
		var jugador = GameManager.Instance.jugadorEnTurno;
		if (indexSubdito < 0 || indexSubdito >= jugador.ColmenaDeReina.AbejasDeColmena.Count) return;
        if (indexCelda < 0 || indexCelda >= tableroActual.Celdas.Count) return;

        SubditoActual = jugador.ColmenaDeReina.AbejasDeColmena[indexSubdito];
        if (!VisualAbejas.TryGetValue(SubditoActual, out var visual) || !IsInstanceValid(visual)) return;
        VisualSubditoActual = visual;

        Celda unaCelda = tableroActual.Celdas[indexCelda];
        MoverSubdito(unaCelda);
	}

	private void MoverSubdito(Celda unaCelda){
    	Vector3 pos = unaCelda.Tile.GlobalPosition;
    	pos.Y = VisualSubditoActual.GlobalPosition.Y;
    	VisualSubditoActual.GlobalPosition = pos;

    	SubditoActual.CeldaActual = unaCelda;
    	celdasManager.PintarCeldaDeAbeja(unaCelda, SubditoActual);

    	_ = GameManager.Instance.ConsumirMovimiento();
    	GameManager.Instance.NotificarCambioDeEstado();
    	AudioManager.Instance.PlaySound(AudioManager.Instance.GameAudio.Sound2);
	}

	public void ControlarAbeja(Celda unaCelda){	
		Abeja subditoAControlar = AbejasCercanas.Find(a => a.CeldaActual == unaCelda && !a.FueraDeJuego);
		
		if (subditoAControlar == null){
    	    EventosUI.MostrarMensaje("Elegí un subdito cercano para controlar.");
    	    return;
    	}
		
		if (!VisualAbejas.TryGetValue(subditoAControlar, out var visual) || !IsInstanceValid(visual)) return;

		GameManager.Instance.CamaraActual.EnfocarNodo(visual, 2, 1);
		GameManager.Instance.jugadorEnTurno.MoviendoSubdito = true;
		VisualSubditoActual = visual;
		SubditoActual = subditoAControlar;
	}

	public bool SubditoTieneAAlguienCerca(Abeja alguien){
		if(SubditoActual == null) return false;
		
		var celdasVecinasDeSubdito = tableroActual.ObtenerVecinos(SubditoActual.CeldaActual);
		
		return celdasVecinasDeSubdito.Contains(alguien.CeldaActual) && alguien != SubditoActual.ColmenaHogar.ReinaDeColmena;
	}

	public bool ExistenSubditosTransformables(){
		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(reinaActual.CeldaActual);

		if (celdasAdyacentes == null || celdasAdyacentes.Any(c => c == null)) return false;

		return celdasAdyacentes.Any(c => reinaActual.ColmenaDeReina.AbejasDeColmena.Any(
			abeja => abeja.CeldaActual == c && abeja.AptaParaTransformar(reinaActual.ColmenaDeReina)
		));
	}

	public void ObtenerSubditosTransformables(){
		if(!ExistenSubditosTransformables()) return;

		AbejasTransformables.Clear();

		var reinaActual = GameManager.Instance.jugadorEnTurno;
		var celdasAdyacentes = tableroActual.ObtenerVecinos(reinaActual.CeldaActual);

		foreach (var celda in celdasAdyacentes){
			var abejaTransformable = reinaActual.ColmenaDeReina.AbejasDeColmena.Find(abeja => abeja is Zangano && abeja.CeldaActual == celda && abeja.AptaParaTransformar(reinaActual.ColmenaDeReina));
			if(abejaTransformable != null) AbejasTransformables.Add(abejaTransformable);
		}
	}

	public void MostrarTransformablesEn(Type unTipoDeAbeja){
		var materialAtaque = GD.Load<StandardMaterial3D>("res://outlineAttack.tres");

		if(ExistenSubditosTransformables()){
			ObtenerSubditosTransformables();
		}
		
		foreach (var abeja in AbejasTransformables){
			if(Activator.CreateInstance(unTipoDeAbeja) is Abeja nuevaAbeja && abeja.AptaParaTransformarEn(nuevaAbeja))        	{
				playerManager.TransformacionSeleccionada = unTipoDeAbeja;
				playerManager.vfxManager.EstablecerNextPass(VisualAbejas[abeja], materialAtaque);
        	}
		}
	}

	public void PrepararTransformaciónDeAbeja(Celda unaCelda){
		var abejaTransformable = AbejasTransformables.Find(a => a.CeldaActual == unaCelda);

		if (abejaTransformable == null){
			EventosUI.MostrarMensaje("Elegí un subdito resaltado para transformar.");
			return;
		}
		
		int indexCelda = tableroActual.Celdas.IndexOf(unaCelda);
		string tipoNombre = playerManager.TransformacionSeleccionada.Name;
		int idJugadorEnTurno = (int)GameManager.Instance.jugadorEnTurno.Id;

		if(GameManager.Instance.modoMultiplayer){
			Rpc(nameof(TransformarAbejaRpc), indexCelda, tipoNombre, idJugadorEnTurno);
		} else {
			TransformarAbeja(unaCelda, playerManager.TransformacionSeleccionada, abejaTransformable, GameManager.Instance.jugadorEnTurno);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void TransformarAbejaRpc(int indexCelda, string tipoNombre, int idJugadorEnTurno){
		if (indexCelda < 0 || indexCelda >= tableroActual.Celdas.Count) return;
		
        Celda unaCelda = tableroActual.Celdas[indexCelda];
        Type tipoTransformacion = Assets.Keys.FirstOrDefault(t => t.Name == tipoNombre);

        if (tipoTransformacion == null) return;

        var jugadorTurno = GameManager.Instance.JugadoresEnPartida.Find(j => j.Id == idJugadorEnTurno);
        if (jugadorTurno == null) return;

        var abejaTransformable = jugadorTurno.ColmenaDeReina.AbejasDeColmena.Find(a => a.CeldaActual == unaCelda);
        if (abejaTransformable == null) return;

        TransformarAbeja(unaCelda, tipoTransformacion, abejaTransformable, jugadorTurno);
	}

	public void TransformarAbeja(Celda unaCelda, Type tipoTransformacion, Abeja abejaTransformable, AbejaReina enTurno){
		if (Activator.CreateInstance(tipoTransformacion) is Abeja abejaNueva){
            GameManager.Instance.TransformarAbeja(abejaTransformable, abejaNueva);

            AbejasCercanas.Remove(abejaTransformable);
            AbejasTransformables.Clear();
            
            if (VisualAbejas.TryGetValue(abejaTransformable, out var visualVieja) && IsInstanceValid(visualVieja)){
                visualVieja.QueueFree();
            }
            VisualAbejas.Remove(abejaTransformable);

            var InstanciaNueva = Assets[tipoTransformacion].Instantiate<Node3D>();

            abejaNueva.ColmenaHogar = enTurno.ColmenaDeReina;
            abejaNueva.CeldaActual = unaCelda;

            AbejasCercanas.Add(abejaNueva);
            VisualAbejas.Add(abejaNueva, InstanciaNueva);
            AlmacenJugadores[enTurno].AddChild(InstanciaNueva);
            EstablecerPosicionDeAbeja(InstanciaNueva, unaCelda.Tile.GlobalPosition);
        }
	}
}