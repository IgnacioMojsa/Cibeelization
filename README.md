# Cibeelization

## Integrantes:
- Marcos Monescao (Programación)
- Maitena Pochyly (Programación)
- Ignacio Mojsa (Programación)
- **[Documento de Proyecto](https://docs.google.com/document/d/1HQicnCwOzRY4aZMAUruPyfpq7V9P2ZZylnPrGp23W18/edit?usp=sharing)**
- **[Backlog Trello](https://trello.com/invite/b/6a9c0252d3f955b72d605526/ATTIf76f71789d58a034bc17964de8fee780C54C9140/programacion-de-videojuegos-ii)**
- **[Builds / Ejecutables](https://drive.google.com/drive/folders/1q2s249DtqBaYbAsVW5poI3B0WpyJeElX?usp=sharing)**

## Pila tecnológica
- Motor: Godot 4.7.2 (C# support)

#### Instrucciones de juego
- Elige la cantidad de jugadores y el tamaño del tablero (opcional).
- Al comenzar la partida, una abeja será asignada a cada jugador, y se indicará su turno con un borde color rojo.
- Cada jugador en su turno deberá tirar los dados y desplazarse, invocar un subdito o atacar en caso de que sea posible según su elección. Si solo elige desplazarse, deberá consumir los movimientos disponibles. Las otras dos acciones terminan el turno inmediatamente.
- Un subdito sólo podrá ser invocado en una de las celdas contiguas al jugador.
- Para atacar a otro jugador, los dos deben estar en celdas contiguas.
- Cada ataque restará 5 de HP. El último jugador que quede con vida será el ganador.

#### Controles
Mouse.
Click derecho para realizar acciones (Botones de la UI) y para realizar movimientos sobre el tablero.
Click izquierdo y arrastre para mover la dirección de la cámara.
<img width="1226" height="267" alt="Image" src="https://github.com/user-attachments/assets/f327d144-96cf-493e-a771-361ff502464a" />

#### MDA Framework
- Mecánicas:
    - Tirar dados
    - Desplazarse
    - Atacar
    - Invocar súbdito

- Dinámicas:
    - Planificar estrategia básica (posicionamiento)
    - Competir contra otros jugadores

- Estéticas:
    - Desafío
    - Azar



## Patrones de diseño [^1]

[^1]: [Referencia de patrones de diseño](https://refactoring.guru/es/design-patterns/catalog)

### Singleton

En nuestro proyecto tenemos 2 clases singleton.
- **GameManager**
- **AudioManager**

Ambas son instancias globales y nos facilitan a coordinar los diferentes scripts independientes, además que nos permite ahorrarnos referencias constantes e innecesarias que podrían sobrecargar nuestro código. Nuestro PlayerManager es el que más uso hace de la instancia global de GameManager, y la GameUI es la que más uso hace del AudioManager respectivamente.

### Observer
Elegimos el patrón de diseño observer ya que lo hemos estado utilizando en la creación del proyecto desde un comienzo.

La interfaz de nuestro juego mantiene informado a todos los jugadores constantemente de:
- Turno del jugador actual
- Número que salió en el dado
- Puntos de vida de cada jugador (HP)
- Instrucciones de juego y mensajes de alerta
- A qué jugador pertenecen las abejas súbditas invocadas en el tablero

La clase GameUI es el observador y TurnManager es el observable. La interfaz del juego mantiene constantemente actualizados a todos los jugadores sobre estado actual del juego. Mediante la UI dinámica, todos los jugadores son notificados automáticamente de cualquier cambio ocurrido en el juego.

En nuestro proyecto, tenemos varios componentes (clases, interfaces y managers) que reaccionan automáticamente a cambios de estados, sobretodo en la clase de TurnManager y los estados del jugador actual (clase AbejaReina).

Nuestra clase TurnManager posee un constructor que recibe como parámetros una lista de jugadores, de alli toma todos los datos y luego GameUI los muestra reflejados en la UI. Esto se realiza mediante el uso de Eventos en C#.
```ruby
public event Action<AbejaReina> OnCambioDeTurnoJugador;
public event Action OnTurnoCambiado;

public TurnManager(List<AbejaReina> jugadores)
   {
       JugadoresEnPartida = jugadores;
   }
```

También, en Godot, la forma en la que conectamos señales con eventos en los botones y en los textos de la UI en Godot son parte del patrón observer. En este caso, nos suscribimos a varios eventos antes de comenzar la partida.
```ruby
private void SuscribirAEventos()
	{
		_ExitTree();

		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar += ActualizarInstrucciones;
		turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
		turnManager.OnTurnoCambiado += ActualizarUI;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
	}
```

Estamos usando esta estructura de código también en nuestra clase PlayerManager.
```ruby
botonPausa.Pressed += PausarPartida;
```

Del mismo modo usamos el método _UnhandledInput (clicks) para notificar a PlayerManager mediante eventos y obtener una respuesta desde esta capa visual.
```ruby
public override void _UnhandledInput(InputEvent @event){...}
```

## Mediador
Implementamos el patrón mediador en nuestro PlayerManager para poder administrar diferentes subsistemas mediante él sin que haya un acoplamiento excesivo en nuestro código.
PlayerManager permite la coordinación de los subsistemas de Tablero, Movimiento, Ataque, Cámara y Tropas con el GameManager global.

Por ejemplo, este método recibe una instancia visual y una ubicación en sus parámetros (obtenida también de un raycast en el PlayerManager). Mueve esa instancia visual a la posición recibida, notifica al GameManager de la acción realizada, actualiza la cámara enfocando al nodo 3D en su nueva posición. También indica al AudioManager que reproduzca un sonido.
```ruby
private void MoverAbejaACelda(Node3D jugador, Celda celdaDestino)
	{
		Vector3 targetPos = celdaDestino.Tile.GlobalPosition;
		targetPos.Y = jugador.GlobalPosition.Y; 
		jugador.GlobalPosition = targetPos;
		CeldaActualPorJugador[jugador] = celdaDestino;
        [...]
		if (index != -1){
			GameManager.Instance.JugadoresEnPartida[index].UbicacionActual = celdaDestino;
			GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 2, 1);
		}

		GameManager.Instance.ConsumirMovimiento();
		GameManager.Instance.NotificarCambioDeEstado();
		[...]
		AudioManager.Instance.PlaySound(sound);
	}
```

En el caso de invocar una abeja nueva, este método permite también obtener una posición en las celdas mediante raycast, consultar dentro de la lista de celdas disponibles y permite que tropasManager se encargue de instanciar la abeja nueva. Oculta los outlines de las abejas objetivo para el cambio de turno y reproduce un sonido.
```ruby
public void InvocarAbejaNueva(){
		Vector2 mousePosition = GetViewport().GetMousePosition();
		Vector3 rayOrigin = camera.ProjectRayOrigin(mousePosition);
		Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePosition) * 1000.0f;
	
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		var result = spaceState.IntersectRay(query);

		if(result.Count == 0) return;

		PosicionEnMundo3D = result["position"].AsVector3();
		CeldaCliqueada = ObtenerCeldaDesdePosicion(tablero.Celdas, PosicionEnMundo3D);
	
		if(CeldaCliqueada == null) return;

		if(CeldasDisponibles.Contains(CeldaCliqueada)){
			OcultarCeldasDisponiblesParaInvocar();	
			OcultarAbejasObjetivo();
			tropasManager.InstanciarAbeja(CeldaCliqueada);
			var bee1 = AudioManager.Instance.GameAudio.Sound5;
			AudioManager.Instance.PlaySound(bee1);
		}
		else{
			EventosUI.MostrarMensaje("No se puede invocar una abeja sobre esta celda");
		}
	}
```
## Integración de Audio, BGM, UISFX y SFX
Se implementó un Audio Manager global que permite manejar AudioStreamPlayer y MusicStreamPlayer.
- Música de fondo: [Watermill in the old town (loopeable) por HarumachiMusic.](https://pixabay.com/nl/music/modern-klassiek-watermill-in-the-old-town-loopable-325123/)
    - Se reproduce constantemente hasta que el juego es pausado. Al volver al menú de inicio, comienza a reproducirse desde cero.
- Sonido UI SFX:
    - Botones generales: [UI Sound 37 por juniorsoundays](https://pixabay.com/sound-effects/film-special-effects-ui-sound-37-527788/)
    - Botón de tirar dado: [dice por JayeWilde (Freesound)](https://pixabay.com/sound-effects/film-special-effects-dice-65736/)
- Game SFX:
    - Movimiento por celdas: [UI Pop Sound](https://pixabay.com/sound-effects/film-special-effects-ui-pop-sound-316482/). Cada movimiento que realice cualquier jugador reproduce este sonido.
    - Ataque: [Punch sound por JakeEaton (Freesound)](https://pixabay.com/sound-effects/film-special-effects-punch-2-37333/). Cuando una abeja ataca a otra, se reproduce este sonido.
    - Invocación de súbditos: [Buzz Cartoon Bug SFX por MRSTOKES302](https://pixabay.com/es/sound-effects/naturaleza-buzz-cartoon-bug-sfx-mrstokes302-427554/). Cuando se invoca un súbdito, se reproduce este sonido.

## Integración de Animaciones y VFX
- Animaciones de esqueleto suaves para todos los modelos 3D de abejas en la escena.
- Outlines para cada jugador y container de jugadores: Refleja en color morado al jugador en turno.
- La cámara 3D enfoca al jugador en turno, orbita suavemente sobre el mismo, y al terminar el turno se desplaza a enfocar al siguiente jugador.
- Outlines de color rojo: Cuando un jugador tiene una abeja enemiga cercana, estas generan un outline color rojo para mostrar que es posible atacarlas. 
- Outlines de diferentes colores para diferenciar a los jugadores.
- Celdas de invocación de color rojo: Cuando un jugador decide invocar un súbdito, se pintan de rojo las celdas donde puede realizar la acción.
- Celdas donde un jugador invocó un súbdito se pintan del mismo color que su outline: esto es para diferenciar las abejas súbditas de cada jugador.


