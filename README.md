# Cibeelization 
## Proyecto final de Programación de Videojuegos II

## Integrantes:
- Marcos Monescao (Programación)
- Maitena Pochyly (Programación)
- Ignacio Mojsa (Programación)
## Links
- **[Game Design Document Actualizado](https://docs.google.com/document/d/1HQicnCwOzRY4aZMAUruPyfpq7V9P2ZZylnPrGp23W18/edit?usp=sharing)**
- **[Backlog en Trello](https://trello.com/invite/b/6a9c0252d3f955b72d605526/ATTIf76f71789d58a034bc17964de8fee780C54C9140/programacion-de-videojuegos-ii)**

## Pila tecnológica
- Motor: Godot 4.7.2 (C# support)
- GDUnit4
- Declaración sobre el uso de IA Generativa: Gemini AI de Google y Claude.

#### Ficha técnica
- Género: Estrategia por turnos (TBS), estrategia en tiempo real (RTS), multijugador local u online de 2, 3 o 4 jugadores.
- Plataformas: PC, con posibilidad de adaptación a Mobile.
- Público objetivo: PEGI 3 Todas las edades, incluye compras.
- Rating estimado: Casual, adolescentes y adultos.
- Unique Selling Point: Juego multijugador con partidas no tan extensas, permite la competitividad sana en base a estrategias.
- Juegos similares: Ajedrez, Civilization, Risk, Age of Empires, Pummel Party.

#### Concepyo y gameplay
##### Pitch
Cibeelization es un juego multijugador competitivo en el que cada jugador podrá controlar una abeja reina y sus súbditas en una colmena, conquistando el territorio o luchando contra los demás jugadores y sus peones. El mismo se desarrolla por turnos, y se utilizan los dados para determinar la cantidad de movimientos del jugador al comienzo de sus turnos. Podrá realizar acciones clave que lo beneficien siempre que tenga movimientos disponibles, entre ellas moverse por el mundo, atacar otras abejas, invocar súbditos, controlarlos, mejorarlos (transformándolos en otros tipos de abejas gastando los recursos producidos) o sacrificarlos para recuperar puntos de vida. 
Posee una estética de gráficos 3D “low poly” animada, controles simples y una ambientación relajada, tanto en lo sonoro como en lo visual. El ciclo de juego es de duración media, permite comprenderlo en un par de minutos aunque requiera el mínimo esfuerzo de memorización. Es ideal para pasar el rato con amigos en casa, como un party game en un solo dispositivo, o a distancia.

##### Core loop
1. Acción: El jugador se mueve, ataca o invoca  y mueve abejas. (tropa)
2. Recompensa: Obtiene recursos para mejorar sus tropas y obtener ventajas, conquista territorio, y se suman chances de declararse ganador.
3. Expansión: Se abren las puertas a nuevos tipos de abeja y mejoras para las tropas.
4. Repetir paso 1.

##### Objetivos
- Objetivo principal: Derrotar a los demás jugadores hasta quedar uno solo de pie. Condición de victoria automática.
- Objetivo secundario: Pintar el mayor porcentaje del tablero. Condición de victoria automática.
- Condición de derrota:
	1. HP del jugador se reduce a cero mediante combate. 
	2. Se acaba el tiempo de partida, lo cual puede desembocar en:
		a. Pierde el jugador con menos HP hasta el momento.
		b. Pierde el jugador con menor porcentaje de tablero pintado.
		c. Empate.

#### Narrativa y mundo
Cibeelization transcurre en un panal al aire libre, donde se ven enfrentadas dos o más abejas reinas que buscan ocupar el mismo lugar. Todas las reinas deberán luchar por ver quién conquista el panal en tan solo 10 minutos.

#### Estilo visual y sonoro
Estilo de Arte: 3D animado (mundo), 2D animado (interfaces). Colores saturados
<img width="1907" height="946" alt="Captura de pantalla de 2026-11-20 02-51-53" src="https://github.com/user-attachments/assets/1cca71c9-d94a-4a5a-bade-598cfe70b5ce" />

Interfaz de usuario: Minimalista, no diegética con algunos elementos espaciales.
Audio:
- Música: Gameplay y menú: Relajada, orquestal, sencilla
- SFX:
	- Movimiento
	- Invocación
	- Ataque
	- Transformación
	- Fin de la partida
	- Interfaces

#### Diseño de niveles
Estructura del mundo: Se puede elegir un único mundo, al iniciar una partida se puede configurar el tamaño del tablero y la cantidad de jugadores (2, 3 o 4).
Biomas y entornos: Tablero en forma de panal con celdas hexagonales al exterior, posee flores rodeando los bordes del mismo.
Curva de dificultad: La dificultad está ligada al tamaño de tablero elegido y la cantidad de jugadores.

#### Reglas del juego
##### Mecánicas básicas
Cada abeja spawnea en el tablero con un HP inicial de 15 puntos. Cada jugador debe empezar su turno lanzando el dado, lo cual habilita el movimiento, el ataque y la función “colmena”. Los movimientos disponibles son el resultado del dado en ese turno.
Durante su turno, mientras tenga movimientos disponibles, el jugador podrá realizar una o más acciones sujetas a diferentes condiciones.
- Si se mueve, se le descontará 1 movimiento por cada celda hexagonal que avance.
- Si decide atacar, podrá atacar una sola vez a cualquier abeja y quitarle 5 puntos de HP (5 puntos de ATK).
- Si decide atacar y también tiene 3 abejas súbditas propias rodeándolo, el ataque se ve incrementado el doble. 
- Si comanda un ataque, todas sus súbditas propias podrán atacar a las abejas enemigas cercanas, aunque su reina esté distanciada de ellas.
- Si presiona el botón “colmena”, podrá acceder a la gestión de tropas.
	- Si no tiene abejas súbditas propias cerca, ni tiene recursos disponibles, sólo podrá invocar un súbdito zángano (peón básico) en su turno.
	- Si ya tiene una abeja súbdita en una celda cercana, podrá elegir las siguientes opciones:
		- Sacrificar abeja, que recupera 5 puntos de HP a cambio de sacrificar su propia abeja súbdita.
		- Controlar abeja, que transfiere el control del movimiento de la reina al súbdito, permitiendo ejecutar ataques manualmente, y con cada movimiento pinta una celda del tablero conquistando territorio. Los movimientos disponibles varían según el tipo de abeja controlada.
		- Transformar abeja, que sólo está disponible si tiene recursos disponibles. Podrá elegir hasta 4 tipos de abejas dependiendo de los recursos que posea actualmente.

##### Tipos de abeja
El juego posee la jerarquía que simula una verdadera colmena de abejas.
1. Reinas: Son los jugadores, que acceden a todas las mecánicas básicas.
2. Abejas súbditas: Forman parte de las tropas invocadas por cada reina. Todas pueden ser controladas en movimiento y ataque manualmente.
Entre las abejas súbditas, están:
- Zángano: Es el peón básico, no tiene costo de invocación. Produce una unidad de agua por turno. Son las únicas abejas que pueden transformarse. Tiene 1 punto de HP y 1 punto de ATK.
- Abeja Obrera: Cuesta 2 unidades de agua y un zángano. Produce una unidad de propóleo por turno.  Es la única abeja que puede romper las barreras de la abeja constructora de reinas enemigas. Tiene 5 puntos de HP y 2 de ATK.
- Abeja Polinizadora: Cuesta 3 unidades de propóleo y un zángano. Produce una unidad de polen por turno. Al controlarla, duplica la cantidad de movimientos disponibles que hayan salido en el dado.  Tiene 3 puntos de HP y 2 puntos de ATK.
- Abeja Constructora: Cuesta 6 unidades de agua y un zángano. Produce una unidad de néctar por turno. Al controlarla, bloquea las celdas por las que se haya movido, y sólo la abeja reina de esa colmena puede caminar sobre ellas. Tiene 2 puntos de HP y 1 de ATK.
- Abeja Guardiana: Cuesta 3 unidades de polen y un zángano. Produce una unidad de néctar por turno. Al controlarla, reduce a la mitad la cantidad de movimientos disponibles que hayan salido en el dado. Tiene 10 puntos de HP y 4 puntos de ATK. (Tanque)

##### Sistema de recursos
El juego posee un sistema de recursos simple que permite la compra y evolución de las abejas zángano. Cada jugador puede visualizar sus propios recursos producidos en la parte superior de la pantalla.
- Agua: El más barato, producido por los zánganos.
- Polen: Producido por la abeja polinizadora.
- Propóleo: Producido por la abeja obrera.
- Néctar: Producido por la abeja constructora y por la abeja guardiana.

#### Matriz MDA (Mechanics - Dynamics - Aesthetics)
##### Mecánicas
Tirar dado
Moverse por las celdas
Bloquear celdas
Pintar y despintar celdas
Atacar otras abejas
Transformar abejas
Recuperar HP
Ganar y gastar recursos
Temporizador de 10 minutos de partida y 30 segundos por turno
##### Dinámicas
Idear estrategias ofensivas o defensivas
Aprovechar los temporizadores para obtener la victoria de diferentes formas
Acumular recursos y  abejas súbditas.
Evitar ataques críticos mediante el movimiento.
Crear barreras de súbditos o celdas 
Conquistar territorio
Competir contra otros jugadores
##### Estéticas
Desafío
Competencia
Azar
Estrés
Alivio
Comunidad

#### Controles
Mouse.
Click derecho para realizar acciones (Botones de la UI) y para realizar movimientos sobre el tablero.
Click izquierdo y arrastre para mover la dirección de la cámara.
<img width="1226" height="267" alt="Image" src="https://github.com/user-attachments/assets/f327d144-96cf-493e-a771-361ff502464a" />
 
#### UML
<img width="3185" height="1876" alt="Diagrama drawio" src="https://github.com/user-attachments/assets/fecf1611-ea4b-4c63-b0d1-b717ec49c2aa" />

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
- Número que salió en el dado y movimientos disponibles
- Puntos de vida de cada jugador (HP)
- Instrucciones de juego y mensajes de alerta
- A qué jugador pertenecen las abejas súbditas invocadas en el tablero

La clase GameUI es el observador y TurnManager es el observable. La interfaz del juego mantiene constantemente actualizados a todos los jugadores sobre estado actual del juego. Mediante la UI dinámica, todos los jugadores son notificados automáticamente de cualquier cambio ocurrido en el juego.

En nuestro proyecto, tenemos varios componentes (clases, interfaces y managers) que reaccionan automáticamente a cambios de estados, sobretodo en la clase de TurnManager y los estados del jugador actual (clase AbejaReina).

Nuestra clase TurnManager posee un constructor que recibe como parámetros una lista de jugadores, de alli toma todos los datos y luego GameUI los muestra reflejados en la UI. Esto se realiza mediante el uso de Eventos en C#.
```ruby
public event Action<AbejaReina> OnCambioDeTurnoJugador;
public event Action OnTurnoCambiado;

public TurnManager(List<AbejaReina> jugadores){
       JugadoresEnPartida = jugadores;
   }
```

También, en Godot, la forma en la que conectamos señales con eventos en los botones y en los textos de la UI en Godot son parte del patrón observer. En este caso, nos suscribimos a varios eventos antes de comenzar la partida.
```ruby
private void SuscribirAEventos(){
		//_ExitTree();
		var turnManager = GameManager.Instance.TurnManager;
		EventosUI.OnMensajeAMostrar += ActualizarInstrucciones;
		if (turnManager != null){
			turnManager.OnCambioDeTurnoJugador += OnCambioDeTurno;
			turnManager.OnTurnoCambiado += ActualizarUI;
		}
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAtaque;
		GameManager.Instance.OnEstadoAccionesCambiado += AlternarEstadoDeAbsorcion;
		GameManager.Instance.OnEstadoAccionesCambiado += ActualizarUI;
	}
```

Estamos usando esta estructura de código también en nuestra clase PlayerManager.
```ruby
botonAtacar.Pressed += OnAtacarPressed;
botonColmena.Pressed += OnBotonColmenaPressed;
botonPausa.Pressed += PausarPartida;
```

Del mismo modo usamos el método _UnhandledInput (clicks) para notificar a PlayerManager mediante eventos y obtener una respuesta desde esta capa visual.
```ruby
public override void _UnhandledInput(InputEvent @event){...}
```

### Mediador
Implementamos el patrón mediador en nuestro PlayerManager para poder administrar diferentes subsistemas mediante él sin que haya un acoplamiento excesivo en nuestro código.
PlayerManager permite la coordinación de los subsistemas de Tablero, Movimiento, Ataque, Cámara y Tropas con el GameManager global.

Por ejemplo, este método recibe una instancia visual y una ubicación en sus parámetros (obtenida también de un raycast en el PlayerManager). Mueve esa instancia visual a la posición recibida, notifica al GameManager de la acción realizada, actualiza la cámara enfocando al nodo 3D en su nueva posición. También indica al AudioManager que reproduzca un sonido.
```ruby
private void MoverAbejaACelda(Node3D jugador, Celda celdaDestino){
		if(GameManager.Instance.jugadorEnTurno.AtaquePotenciado){
			celdasManager.DespintarCeldaDeAbejaBuffeada();
		}
		Vector3 targetPos = celdaDestino.Tile.GlobalPosition;
		targetPos.Y = jugador.GlobalPosition.Y; 
		jugador.GlobalPosition = targetPos;
		CeldaActualPorJugador[jugador] = celdaDestino;
		int index = VisualesJugadores.Values.ToList().IndexOf(jugador);
		if (index != -1){
			GameManager.Instance.JugadoresEnPartida[index].CeldaActual = celdaDestino;
			if(EsTurnoLocal()){
				GameManager.Instance.CamaraActual.EnfocarNodo(VisualJugadorActual, 2, 1);
			}
		}
		_ = GameManager.Instance.ConsumirMovimiento();
		GameManager.Instance.NotificarCambioDeEstado();
		AudioManager.Instance.PlaySound(Sound2);
	}
```

En el caso de invocar una abeja nueva, este método permite también obtener una posición en las celdas mediante raycast, consultar dentro de la lista de celdas disponibles y permite que tropasManager se encargue de instanciar la abeja nueva. Oculta los outlines de las abejas objetivo para el cambio de turno y reproduce un sonido.
```ruby
public void InvocarAbejaNueva(){
		CeldaCliqueada = ObtenerCeldaAlClickear();
		if(celdasManager.CeldasDisponibles.Contains(CeldaCliqueada)){
			celdasManager.OcultarCeldasDisponiblesParaInvocar();	
			vfxManager.OcultarAbejasObjetivo();
			tropasManager.InstanciarAbeja(CeldaCliqueada);
			var bee1 = AudioManager.Instance.GameAudio.Sound5;
			AudioManager.Instance.PlaySound(bee1);
		}
		else{
			GD.Print("No se puede invocar una abeja sobre esta celda");
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
- Splash Screen de video personalizado al abrir el juego. 
- Animaciones de esqueleto suaves para todos los modelos 3D de abejas en la escena. Todas las abejas poseen una animación Idle. Las abejas reinas poseen una animación cuando reciben daño, y la abeja de tipo Guardiana posee una animación cuando ataca, para hacer énfasis en que es una especie de tanque.
- Outlines para cada jugador y container de jugadores: Cada jugador posee un outline de su color asignado automáticamente al ingresar a la partida, cuando decide atacar manualmente o controlar a sus súbditos, las abejas enemigas activan un borde color rojo siempre que sea posible seleccionarlas o hacerles click encima.
- La cámara 3D enfoca al jugador en turno, orbita suavemente sobre el mismo, y al terminar el turno se desplaza a enfocar al siguiente jugador. Posee rotación sobre  el eje X e Y manualmente. En el modo multijugador, cuando se activa el modo espectador o no es tu turno, se permite el desplazamiento de cámara para espectar a las demás abejas reinas.
- Celdas de invocación de color rojo: Cuando un jugador decide invocar un súbdito, se pintan de rojo las celdas donde puede realizar la acción.
- Celdas donde un jugador invocó un súbdito se pintan del mismo color que su outline: esto es para diferenciar las abejas súbditas de cada jugador y  para permitir la pintada del tablero, conquistando terreno y obteniendo una mayor probabilidad de victoria.
- Celdas buffeadas: Al estar rodeado de tres súbditos propios, la celda de la abeja reina actual se pinta de color arcoiris, pasando por los 6 colores constantemente para indicar que su ataque está potenciado al doble.


## Reporte de testings
Realizamos tests sobre nuestras clases TurnManager y GameManager.

#### TurnManagerTests

Nuestro TurnManager nos permite gestionar el flujo secuencial estricto de los turnos de cada jugador y qué acciones puede realizar durante el mismo. Antes de empezar con los tests, armamos un escenario de partida de 3 jugadores, donde el jugador 3 está fuera de turno.

Con el test case AsignarJugadorCorrectoEnElPrimerTurno() utilizamos esa partida para ejecutar el método EstablecerPrimerTurno().
El test valida que el índice del primer turno sea 0 (int), que el índice actual de la lista de jugadores sea 0, que el estado EsSuTurno del jugador actual pase a ser true, y que el estado EstadoTurno sea EsperandoDado.

Con el test case de OmitirJugadorFueraDeJuegoAlCambiarDeTurno(), validamos que el paso del turno del jugador 2 sea hacia el jugador 1, ya que el jugador 3 está fuera de juego. 

Estas pruebas unitarias realizadas con GDUnit4 y C# nos permitieron confirmar la correcta gestión y asignación de los jugadores en juego y fuera de juego. También nos permitió verificar el correcto funcionamiento de nuestra máquina de estados EstadoTurno de nuestra clase AbejaReina. Y finalmente, nos permitió probar el comportamiento cíclico de nuestra lista de turnos para evitar los errores de índices inexistentes.

#### GameManagerTests

Nuestro GameManager es una instancia global y singleton que controla el estado del juego, administra la partida a gran escala: los turnos, los movimientos y el fin del juego. Para testear la clase, armamos dos simulaciones de partida diferentes para dos tests diferentes.

En el test case ConsumirMovimientosEnUnTurnoYPasarAlSiguienteJugador() simulamos una partida de 2 jugadores, al jugador actual se le establecen 3 movimientos restantes y luego realiza 3 movimientos con el método ConsumirMovimiento(). Este test nos permite validar que agotó los 3 movimientos y que el turno dejó de ser suyo, para pasar a ser del siguiente jugador.

El test case QuedaUnJugadorEnPieYSeCumpleLaCondicionDeVictoria() simula una partida con 4 jugadores donde 3 quedan fuera de juego. Luego validamos que la condición de victoria se cumpla (1 solo jugador en pie), finalmente establecemos al jugador ganador con el método EstablecerJugadorGanador() y validamos que sea el jugador ganador tenga la misma ID que el último jugador en pie.

Estos dos tests nos permitieron verificar el correcto funcionamiento de los movimientos disponibles, y que no permita movimientos infinitos. También nos permite verificar que todo funciona correctamente para finalizar el ciclo de juego estableciendo a un ganador. También, hicimos uso del método ResetearEstadoPartida() en el BeforeTests y en el AfterTest para limpiar los datos de la partida y crear dos simulaciones diferentes.

## Implementación de red
### Clase MultiplayerController
Dentro de la capa visual creamos una clase MultiplayerController, que hereda de Node y fue asignada como script principal a una escena “Lobby” de tipo control.
Establecimos el puerto 8910 y la dirección IP local, 127.0.0.1, junto a unos botones para comprobar los primeros métodos de red que implementamos:
- PeerConnected
- PeerDisconnected
- ConnectedToServer
- ConnectionFailed.
Estos métodos son conectados al nodo Multiplayer de Godot apenas se ingresa a la escena de lobby, mediante el _Ready().
En la escena de lobby agregamos tres botones: Host, Cliente y Jugar, que próximamente serían renombrados como “Crear partida” (Host) y “Unirme a una partida” (Cliente). 

Al presionar el botón Host, se instancia un nuevo MultiplayerPeer y crea el server utilizando el puerto elegido y limitando los clientes a cuatro. Le aplicamos un modo de compresión RangeCoder.
Al presionar el botón Cliente, se instancia un nuevo MultiplayerPeer y crea un cliente con la dirección IP y el puerto asignado. También le aplicamos compresión de RangeCoder.
Al presionar el botón Jugar, se ejecuta el método RPC startGame. startGame usa una RPC (remote procedure call) donde el Api está habilitado para cualquier peer, puede llamar localmente y usa un modo de transferencia Reliable. 
```ruby
[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
```
El TransferModeEnum “Reliable” permite recibir información en el orden en el que fue enviada, con precisión ideal para salas de chat y mensajería que nos servirían en el futuro desarrollo de nuevas funciones.

startGame permite crear la partida con los datos obtenidos en la pantalla de inicio, ya que el GameManager guarda las variables que tienen los datos de la cantidad de jugadores elegidos para la partida, el tamaño de tablero escogido, y procede a instanciar el TurnManager para establecer el primer turno antes de cambiar a la escena del juego.

Posteriormente realizamos ajustes en la lógica de los botones integrados, mediante el uso de variables globales “modoMultiplayer” y “jugadorEsHost” como flags.
En caso de que el jugador, desde pantalla de inicio, decida crear una partida, será transportado al lobby y sólo tendrá habilitados los botones “Crear partida” y “Jugar”.
En cambio, si un jugador, desde la pantalla de inicio, decide unirse a una partida, será transportado al lobby y sólo tendrá habilitado el botón “Unirme a una partida”.
Esto, en el futuro, nos permitirá incorporar la creación de salas individuales y el poder decidir manualmente a cuál unirnos.

Realizamos cambios en la lógica de conexión y desconexión, empezando por incorporar el método ServerDisconnected, que se conecta a su par Multiplayer.ServerDisconnected mediante el _Ready(). Este método permite desconectar a los clientes en caso de que el host dé de baja el server o se desconecte.

Creamos un script nuevo para asignar a los nodos de la interfaz que incorporamos, “lobbyUI.cs”. Agregamos las opciones de audio, resolución de pantalla y un botón para salir del lobby y regresar a la pantalla inicial. Con el método LimpiarYVolverAlMenu desconectamos a los peers que hayan entrado al servidor, sean host o clientes. El método ServerDisconnected ahora llama a este otro método para dar de baja la conexión del server.
Al regresar a la pantalla de inicio, todos los datos guardados en el GameManager sobre la configuración de la partida, se resetean con el método ResetearEstadoPartida, así evitamos errores de múltiples configuraciones de partidas online con datos erróneos.

Limitamos la cantidad de jugadores que pueden unirse al servidor mediante el botón Crear Partida, que llama al método “on_host_button_down”, donde se crea el servidor con los siguientes datos: puerto y cantidad máxima de clientes.
Utilizamos la función matemática Clamp para establecer que el mínimo sea  y el máximo sea 4. Al crear el servidor, el máximo de clientes debe ser siempre 1 menos porque el host no es un cliente.

Como un detalle estético y acorde a la temática del juego, incorporamos los modelos 3D de las abejas en la sala de espera, que se sincronizan perfectamente cuando entran o salen del lobby. Para esto, utilizamos un método ActualizarAbejasEnEspera, y una lista de abejasEnEspera, que posee los 4 modelos 3D de las abejas. Al unirse un jugador, el modelo se vuelve visible, y al desconectarse, se oculta. Cada abeja muestra el nombre que el jugador eligió para unirse al lobby.

Dentro de nuestra escena de juego tenemos un temporizador de turnos. Decidimos usar ese temporizador para permitir la reconexión de los jugadores que hayan perdido la conexión o se les haya cerrado el juego en medio de la partida.
Decidimos brindarle un poco más de tiempo al de un solo turno, por lo que utilizamos el doble del tiempo del temporizador de turno para esperar una reconexión.
Esta parte la manejamos con la clase Timers, que tiene tres métodos específicos para la reconexión:
- IniciarTimerReconexion, que duplica el tiempo del timer de turnos.
- CancelarTimerReconexion, que detiene el timer de reconexión cuando el jugador vuelve a unirse exitosamente.
- OnTimerReconexionTimeout, que mediante el GameManager elimina al jugador al acabarse el timer, si no logró volver a reconectarse con éxito. Si la partida tiene 3 o 2 jugadores aún, puede continuar, pero si solo quedó 1, se le otorga la victoria a este.
En nuestra clase MultiplayerController manejamos dos métodos Rpc para reconectar al jugador.
- ActualizarIDDeJugadorEnReconexion, que actualiza la nueva ID del jugador que se desconectó, ya que las UniqueID de los jugadores son únicas y no se pueden guardar cuando hay una desconexión, simplemente, se le otorga una nueva y hay muy pocas probabilidades de que coincida con la anterior que tenía.
- ReconectarClienteAPartida, que tras presionar el botón de unirse a una partida de nuevo, puede aparecer de vuelta en el tablero con el estado actual del mismo y seguir jugando.

Lo último que implementamos dentro de nuestra clase MultiplayerController, fue la posibilidad de agregar jugadores como espectadores.
Al crear una partida online, el jugador podrá activar la opción “permitir espectadores” y podrá agregar hasta 10 espectadores para una sola partida (cupo de espectadores)-
Si permite espectadores, los jugadores que se unan a la sala podrán elegir presionar el botón “Observar partida”, sin necesidad de que ya haya comenzado. Esto está inspirado en la creación de partidas del videojuego Dota II.
Para permitir la entrada de espectadores, se agregó una lista de espectadores en el GameManager, simulando la lista de JugadoresEnPartida. También, el servidor se crea en base al cupo de espectadores.
``` ruby
peer.CreateServer(port, maxClientes + cupoDeEspectadores);
```
De este modo, los espectadores pasan a ser clientes del servidor, por lo que la cámara estará siempre sincronizada en el jugador en turno, y podrán desplazarse con las teclas A y D por el resto de jugadores.

### Sincronización
Lo primero que hicimos para la sincronización en multijugador fue agregar el nodo MultiplayerSpawner en la escena de juego. Agregamos un nodo de MultiplayerSyncronizer a cada escena de abejas reinas (jugadores) para que sincronicen posición y rotación.

En la clase PlayerManager, agregamos los primeros métodos Rpc. Todos, tienen la siguiente configuración:
``` ruby
[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
```
El RpcMode AnyPeer permite que el método pueda ser llamado remotamente por cualquier peer, ya sea host o cliente.
CallLocal = true permite que el método pueda ser llamado localmente, sino, lo hará de forma remota.
Y el TransferMode Reliable permite recibir paquetes o información en el orden en el que fueron enviados, lo cual es clave.
- MoverAbejaACeldaRpc: Este método es llamado dentro del método que ya teníamos en el modo de juego local, IntentarMoverJugador, sólo cuando la variable global modoMultiplayer es true. Conservamos el método MoverAbejaACelda para ser llamado en el modo de juego local, ya que tiene una estructura muy distinta.
- SiguienteTurnoRpc: Este método actualiza correctamente los turnos de los jugadores en tiempo real y desencadena el cambio del enfoque de la cámara.
- SincronizarDadoRpc: Habiendo deshabilitado la interfaz para los jugadores cuando no es su turno en multijugador, con este método le permitimos ver en tiempo real el resultado del dado que haya sacado una reina rival, y actualiza los movimientos restantes en la pantalla para todos los jugadores.
- SincronizarRecursosRpc: Fue implementado mucho después, cuando terminamos de crear el sistema de recursos para el juego en modo local. Para el diseño del juego.
Continuamos con la implementación de métodos Rpc en la clase TroopsManager:
- InstanciarAbejaRpc: Que instancia una abeja haciendo uso de método InstanciarAbejaLocal. Con esto, sincronizamos la invocación de abejas súbditas.
- AbsorberSubditoRpc: Al igual que el anterior, hace udo del método AbsorberSubditoLocal. De este modo, sincronizamos el “sacrificio de abejas”, eliminando las instancias visuales cada vez que se ejecutaba esa opción.
- MoverSubditoRpc: Con este método, sincronizamos el movimiento de los súbditos al controlarlo, haciendo uso del método local MoverSubdito. No fue necesario sincronizar los colores de celdas pintadas, ya que esos datos se guardaban dentro del GameManager. Lo que sí sincronizamos en este método fue la aparición de la celda buffeada, para que ambos jugadores sepan que está ocurriendo el ataque potenciado y no se lleven sorpresas inesperadas.
- TransformarAbejaRpc: Para terminar con las tropas de los jugadores, este método sincroniza correctamente la transformación de una abeja a otra.
Para finalizar, lo último que sincronizamos fueron los ataques. Desde las animaciones de ataque y daño, hasta la interfaz meta que reflejaba la vida de los jugadores.
- AtacarAbejaRpc: Al procesar el ataque de la abeja, se ejecuta este método para actualizar los visuales de los súbditos en caso de que hayan sido eliminados por un ataque (similar a Absorber súbdito). Reproduce sus animaciones al ser dañadas y los efectos visuales.
- AtacarJugadorRpc: Al atacar a un jugador, se sincronizan las animaciones y efectos visuales de cuando los jugadores reciben daño y procesa el daño recibido. Al igual que el método anterior, actualiza los visuales de los jugadores en caso de que sean eliminados, aunque eso sólo se puede observar en una partida de más de tres jugadores.
- AtacarConGuardianaRpc: Método especial dedicado a la animación de ataque de la abeja guardiana, ya que es la única abeja súbdita que posee una propia. Se encarga principalmente de sincronizar la animación cuando se realiza un ataque normal y la guardiana está siendo controlada.
- ReproducirAnimacionAtaqueSubditoRpc: El método encargado de sincronizar la animación de ataque de la abeja guardiana solo cuando se realiza un ataque comandado, es decir, desde la distancia de su jugador dueño.
- ComandarAtaqueSubditosRpc: Este método se encarga de gestionar los visuales de las abejas súbditas al recibir un ataque comandado. Las abejas que mueran con el ataque serán eliminadas, y las que sigan de pie reproducirán sus animaciones al recibir año.
- ComandarAtaqueAJugadorRpc: Recrea lo mismo que el metodo anterior pero aplicado a los jugadores. Gestiona los visuales al recibir un ataque, reproduce las animaciones, o elimina sus instancias si mueren y están fuera de juego. Este método, específicamente, actualiza la vida de los jugadores presente como parte de una interfaz meta, debajo de las mismas.

## Declaración sobre uso de IA generativa
Nuestro proyecto utilizó inteligencia artificial generativa principalmente para lectura y búsqueda de errores en código. Los modelos utilizados fueron Gemini AI y Claude.

Nos ayudamos de la misma para verificar errores de tipeo, que eran muy comunes y desembocaban en errores de “null” o “not found” en el depurador, pero que pasaban desapercibidos en la terminal de Visual Studio Code o el Output de Godot.

Y también fue utilizada para corregir bloques de código donde debíamos validar si la instancia buscada no era nula antes de intentar ejecutar algún método, que también era otro problema recurrente. Fue así que empezamos a utilizar métodos como “Obtener elemento…” y verificar si la instancia no era nula antes de aplicarle la lógica deseada.

Por otro lado, también se utilizó para entender la lógica de algunos scripts bastante antiguos al tener que realizar cambios.

Para el primer parcial, la clase “Tablero” fue codeada con inteligencia artificial generativa, principalmente para la lógica matemática y trigonométrica al crear celdas hexagonales (cuando las más comunes en los videojuegos son las celdas cuadradas o rectangulares). También tuvimos de apoyo muchos tutoriales y videos disponibles en YouTube, con una idea muy similar a lo que buscábamos lograr.

Lo mismo para la clase “CameraController”, ya que contiene Raycast con el mouse y necesitábamos comprender la lógica detrás del mismo. El método que ajusta la cámara según el tamaño de tablero, también, fue codeado con inteligencia artificial. Y la posibilidad de rotar la cámara con el mouse sobre el eje X e Y también.

Recientemente fue utilizada para refactorizar el script de la clase GameUI y llevar a cabo correctamente la configuración del juego, obteniendo correctamente los nodos raíces buscados de nuestros principales Managers e instancias de HUD superior e inferior.
