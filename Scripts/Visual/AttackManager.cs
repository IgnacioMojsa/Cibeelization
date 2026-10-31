using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AttackManager : Node3D
{
	[Export] private PlayerManager playerManager;
	[Export] public Camera3D camera;
	public readonly List<Abeja> AbejasObjetivo = new();
	private AttackLogic attackLogic;

	public override void _Ready(){
		attackLogic = new AttackLogic();
	}

	public void Atacar(){
		if (!GameManager.Instance.PuedeAtacar())
			return;

		//Camera3D camera = playerManager.camera ?? GetViewport().GetCamera3D();
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0)
		return;

		playerManager.PosicionEnMundo3D = result["position"].AsVector3();
		
		Node nodoCollider = result["collider"].As<GodotObject>() as Node;
		Abeja abejaCliqueada = ObtenerAbejaDesdeCollider(nodoCollider);

		if(abejaCliqueada != null){
			playerManager.CeldaCliqueada = abejaCliqueada.CeldaActual;
			playerManager.RotarJugadorHex(playerManager.VisualJugadorActual, playerManager.CeldaCliqueada);
			if (AbejasObjetivo.Contains(abejaCliqueada)){
				AtacarAbejaEspecifica(abejaCliqueada);
				return;
			}
		}

		playerManager.CeldaCliqueada = playerManager.ObtenerCeldaDesdePosicion(playerManager.tablero.Celdas, playerManager.PosicionEnMundo3D);
		if (playerManager.CeldaCliqueada == null)
		return;

		playerManager.RotarJugadorHex(playerManager.VisualJugadorActual, playerManager.CeldaCliqueada);

		var subditoAdyacenteAJugador = GameManager.Instance.JugadoresEnPartida.Any(j => playerManager.tropasManager.SubditoTieneAAlguienCerca(j));

		if(OtroJugadorCerca() || subditoAdyacenteAJugador){
			AtacarJugador();
		}
		else if(AbejasObjetivo.Count > 0){
			AtacarAbeja();
		}
		
		playerManager.vfxManager.OcultarAbejasObjetivo();
	}


	private Abeja ObtenerAbejaDesdeCollider(Node collider){
		if(collider == null || playerManager.tropasManager == null || playerManager.tropasManager.VisualAbejas == null)
		return null;

		Node actual = collider;

		while(actual != null && actual != GetTree().Root){
			if(actual is Node3D node3d){
				var par = playerManager.tropasManager.VisualAbejas.FirstOrDefault(kvp => kvp.Value == node3d);
				if(par.Key != null){
					return par.Key;
				}
			}
			actual = actual.GetParent();
		}
		return null;
	}

	public bool OtroJugadorCerca(){
		return playerManager.VisualesJugadores.Values.Any(v => playerManager.JugadorEnTurnoAdyacenteAOtro(v));
	}

	public bool TieneObjetivosCerca(){	
		ActualizarAbejasObjetivo();
		return AbejasObjetivo.Count > 0 || OtroJugadorCerca();
	}

	private void AtacarAbejaEspecifica(Abeja abejaObjetivo){
		if(abejaObjetivo == null) return;
		ProcesarAtaqueDeAbeja(abejaObjetivo);
	}

	private void AtacarAbeja(){
		var abejaObjetivo = AbejasObjetivo.Find(a => a.CeldaActual == playerManager.CeldaCliqueada);

		if(abejaObjetivo == null){
			GD.Print("No hay ninguna abeja objetivo en la celda seleccionada.");
			EventosUI.MostrarMensaje("No hay ninguna abeja enemiga para atacar ahí.");
			return;
		}

		ProcesarAtaqueDeAbeja(abejaObjetivo);
		playerManager.TerminarControlDeSubdito();
	}

	private void ProcesarAtaqueDeAbeja(Abeja abejaObjetivo){
		int indexJugadorRival = -1;
        int indexAbejaRival = -1;

        for (int i = 0; i < GameManager.Instance.JugadoresEnPartida.Count; i++){
            var jugador = GameManager.Instance.JugadoresEnPartida[i];
            int index = jugador.ColmenaDeReina.AbejasDeColmena.IndexOf(abejaObjetivo);
            if (index != -1){
                indexJugadorRival = i;
                indexAbejaRival = index;
                break;
            }
        }

        if (indexJugadorRival == -1 || indexAbejaRival == -1) return;
        if (GameManager.Instance.modoMultiplayer){
            Rpc(nameof(AtacarAbejaRpc), indexJugadorRival, indexAbejaRival);
        } else {
            AtacarAbejaRpc(indexJugadorRival, indexAbejaRival);
        }
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void AtacarAbejaRpc(int indexJugadorRival, int indexAbejaRival){
		if (indexJugadorRival < 0 || indexJugadorRival >= GameManager.Instance.JugadoresEnPartida.Count) return;
        var jugadorRival = GameManager.Instance.JugadoresEnPartida[indexJugadorRival];

        if (indexAbejaRival < 0 || indexAbejaRival >= jugadorRival.ColmenaDeReina.AbejasDeColmena.Count) return;
        var abejaObjetivo = jugadorRival.ColmenaDeReina.AbejasDeColmena[indexAbejaRival];

        Abeja abejaControlada = GameManager.Instance.jugadorEnTurno.MoviendoSubdito ?
		playerManager.tropasManager.SubditoActual : GameManager.Instance.jugadorEnTurno;
        
        attackLogic.DaniarAbeja(abejaObjetivo, abejaControlada);

        if(abejaObjetivo.FueraDeJuego && playerManager.tropasManager.VisualAbejas.TryGetValue(abejaObjetivo, out Node3D visual) && IsInstanceValid(visual)){
            playerManager.celdasManager.DespintarCeldaDeAbeja(abejaObjetivo.CeldaActual);
            visual.QueueFree();
            playerManager.tropasManager.AbejasCercanas.Remove(abejaObjetivo);
            playerManager.tropasManager.VisualAbejas.Remove(abejaObjetivo);
        }
        
        LimpiarAbejasEliminadas();
        
        if(GameManager.Instance.jugadorEnTurno.MoviendoSubdito){
            GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(playerManager.VisualJugadorActual);
            GD.Print("Cámara restablecida en reina");
        }
        GameManager.Instance.ConsumirAtaque();
	}

	private void AtacarJugador(){
        playerManager.VisualJugadorActual = playerManager.VisualesJugadores[GameManager.Instance.jugadorEnTurno];
        Abeja atacante;
        Celda celdaAtacante;

        if (GameManager.Instance.jugadorEnTurno.MoviendoSubdito){
            atacante = playerManager.tropasManager.SubditoActual;
            celdaAtacante = atacante.CeldaActual;
            GameManager.Instance.CamaraActual.ResetearCamaraAutomaticamente(playerManager.VisualJugadorActual);
        }
        else{
            atacante = GameManager.Instance.jugadorEnTurno;
            celdaAtacante = playerManager.CeldaActualPorJugador[playerManager.VisualJugadorActual];
        }

        List<Celda> celdasAdyacentes = playerManager.tablero.ObtenerVecinos(celdaAtacante);

        AbejaReina reinaObjetivo = GameManager.Instance.JugadoresEnPartida.Find(
            j => !j.FueraDeJuego && j != GameManager.Instance.jugadorEnTurno 
            && j.CeldaActual == playerManager.CeldaCliqueada);

        if (reinaObjetivo != null){
            if (celdasAdyacentes.Any(c => c == reinaObjetivo.CeldaActual)){
                
                int indiceRival = GameManager.Instance.JugadoresEnPartida.IndexOf(reinaObjetivo);

                if (GameManager.Instance.modoMultiplayer){
                    Rpc(nameof(AtacarJugadorRpc), indiceRival);
                }
                else{
                    AtacarJugadorRpc(indiceRival);
                }

                GameManager.Instance.ConsumirAtaque();
            }
            else{
                GD.Print("La Abeja Reina rival está demasiado lejos para ser atacada.");
                EventosUI.MostrarMensaje("Esa abeja enemiga está muy lejos.");
            }
        }
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void AtacarJugadorRpc(int indiceRival){
        if (indiceRival < 0 || indiceRival >= GameManager.Instance.JugadoresEnPartida.Count) return;

        AbejaReina reinaRival = GameManager.Instance.JugadoresEnPartida[indiceRival];
        //Node3D visualRival = playerManager.VisualesJugadores[reinaRival];

        Abeja atacante = GameManager.Instance.jugadorEnTurno.MoviendoSubdito ? 
		playerManager.tropasManager.SubditoActual : GameManager.Instance.jugadorEnTurno;

        ProcesarDanioJugador(indiceRival, atacante);

        playerManager.animationManager.CambiarAnimacionDeJugador(reinaRival, "RecibirDanio");
        playerManager.tropasManager.OcultarAbejasCercanas();
    }

    private void ProcesarDanioJugador(int indiceJugador, Abeja abejaAtacante){
        if (indiceJugador < 0 || indiceJugador >= GameManager.Instance.JugadoresEnPartida.Count) return;
        AbejaReina jugador = GameManager.Instance.JugadoresEnPartida[indiceJugador];

        if(jugador != null){
            attackLogic.DaniarJugador(abejaAtacante, jugador);
            GD.Print("¡Ataque exitoso! Jugador " + jugador.Id + " ahora tiene " + jugador.HP + " puntos de vida");
        }

        Node3D visualRival = playerManager.VisualesJugadores[jugador];
        if(attackLogic.JugadorEstaEliminado(jugador)){
            EliminarInstanciaDeJugador(visualRival, indiceJugador);
        }
    }

	private void EliminarInstanciaDeJugador(Node3D unJugador, int Id){
		GameManager.Instance.EliminarJugador(Id);
		unJugador.Visible = false;

		if(GameManager.Instance.CondicionVictoria()){
			GameManager.Instance.EstablecerJugadorGanador();
			GetTree().ChangeSceneToFile("res://Scenes/pantallaVictoria.tscn"); 
		};
	}

	public void BuscarAbejasObjetivo(AbejaReina jugadorRival, List<Abeja> listaDeAbejas){
		Abeja jugadorActual;

		if (GameManager.Instance.jugadorEnTurno.MoviendoSubdito){
			jugadorActual = playerManager.tropasManager.SubditoActual;
		}
		else
		{
			jugadorActual = GameManager.Instance.jugadorEnTurno;
		}
		
		foreach (var abejaActual in jugadorRival.ColmenaDeReina.AbejasDeColmena){
			var reinaEsDueñaDeUnSubdito = false;

			if(jugadorActual is AbejaReina reinaJugadorActual){
				reinaEsDueñaDeUnSubdito = reinaJugadorActual.ColmenaDeReina.AbejasDeColmena.Contains(abejaActual);
			}
			
			if (playerManager.movimientoManager.CeldasSonAdyacentes(jugadorActual.CeldaActual, abejaActual.CeldaActual) 
			&& !abejaActual.FueraDeJuego
			&& (!reinaEsDueñaDeUnSubdito)){
				listaDeAbejas.Add(abejaActual);
			}
		}
	}

	public void ActualizarAbejasObjetivo(){
		AbejasObjetivo.Clear();
		playerManager.VisualJugadorActual = playerManager.VisualesJugadores[GameManager.Instance.jugadorEnTurno];

		foreach (var jugador in GameManager.Instance.JugadoresEnPartida){
			if (jugador == GameManager.Instance.jugadorEnTurno || jugador.FueraDeJuego) continue;
			BuscarAbejasObjetivo(jugador, AbejasObjetivo);
		}
		foreach (var abeja in AbejasObjetivo){
			GD.Print(abeja.GetType() + ", ", abeja.ColmenaHogar.ReinaDeColmena.GetType() + " " + abeja.ColmenaHogar.ReinaDeColmena.Id);
		}
	}

	public void LimpiarAbejasEliminadas(){
		foreach (var abeja in AbejasObjetivo.Where(a => a.FueraDeJuego).ToList()){
			if(abeja.FueraDeJuego){
				AbejasObjetivo.Remove(abeja);
				abeja.ColmenaHogar.AbejasDeColmena.Remove(abeja);
			}
			AbejasObjetivo.RemoveAll(a => a.FueraDeJuego);
		}
	}
}
