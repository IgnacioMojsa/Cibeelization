# Abeja

## Integrantes 
- Marcos Monescao
- Maitena Pochyly
- Ignacio Mojsa

### [Documento de Proyecto](https://docs.google.com/document/d/1HQicnCwOzRY4aZMAUruPyfpq7V9P2ZZylnPrGp23W18/edit?usp=sharing)
### [Backlog Trello](https://trello.com/invite/b/6a9c0252d3f955b72d605526/ATTIf76f71789d58a034bc17964de8fee780C54C9140/programacion-de-videojuegos-ii)
### [Builds / Ejecutables](https://drive.google.com/drive/folders/1q2s249DtqBaYbAsVW5poI3B0WpyJeElX?usp=sharing)

## Instrucciones de juego
- Elige la cantidad de jugadores y el tamaño del tablero (opcional).
- Al comenzar la partida, una abeja será asignada a cada jugador, y se indicará su turno con un borde color rojo.
- Cada jugador en su turno deberá tirar los dados y desplazarse, invocar un subdito o atacar en caso de que sea posible según su elección. Si solo elige desplazarse, deberá consumir los movimientos disponibles. Las otras dos acciones terminan el turno inmediatamente.
- Un subdito sólo podrá ser invocado en una de las celdas contiguas al jugador.
- Para atacar a otro jugador, los dos deben estar en celdas contiguas.
- Cada ataque restará 5 de HP. El último jugador que quede con vida será el ganador.

## MDA Framework
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

## Patrón de Diseño Observer
Elegimos el patrón de diseño observer ya que lo hemos estado utilizando en la creación del proyecto desde un comienzo.

La interfaz de nuestro juego mantiene informado a todos los jugadores constantemente de:
- Turno del jugador actual
- Número que salió en el dado
- Puntos de vida de cada jugador (HP)

Además, se muestra un texto de instrucciones que indica qué puede hacer y qué no el jugador actual en tiempo real.

La clase GameUI es el observador y TurnManager es el observable. La interfaz del juego mantiene constantemente actualizados a todos los jugadores sobre estado actual del juego. Mediante la UI dinámica, todos los jugadores son notificados automáticamente de cualquier cambio ocurrido en el juego.

En nuestro proyecto, tenemos varios componentes (clases, interfaces y managers) que reaccionan automáticamente a cambios de estados, sobretodo en la clase de TurnManager y los estados del jugador actual (clase AbejaReina).

Nuestra clase TurnManager posee un constructor que recibe como parámetros una lista de jugadores, de alli toma todos los datos y luego GameUI los muestra reflejados en la UI.
```ruby
public TurnManager(List<AbejaReina> jugadores)
   {
       JugadoresEnPartida = jugadores;
   }
```

También, en Godot, la forma en la que conectamos señales con eventos en los botones de la UI en Godot tienen un funcionamiento similar al patrón observer. Estamos usando esta estructura de código también en nuestra clase PlayerManager.

```ruby
botonPausa.Pressed += PausarPartida;
```

Del mismo modo usamos el método _UnhandledInput (clicks) para notificar a PlayerManager y obtener una respuesta desde esta capa visual.

A futuro tenemos pensado implementar un patrón de diseño strategy para la obtención de recursos y manejo de los distintos tipos de abejas.


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
    - Invocación de súbditos: [Minecraft Bee Sounds](https://tuna.voicemod.net/sound/980b72c9-fed9-405e-b604-af1bfd7d318a). Cuando se invoca un súbdito, se reproduce este sonido.

## Integración de Animaciones y VFX
- Outlines para cada jugador y container de jugadores: Refleja en color morado al jugador en turno.
- La cámara 3D enfoca al jugador en turno y cuando se mueve.
- Celdas de invocación de color rojo: Cuando un jugador decide invocar un súbdito, se pintan de rojo las celdas donde puede realizar la acción.
- Outlines de color rojo: Cuando un jugador tiene una abeja enemiga cercana, estas generan un outline color rojo para mostrar que es posible atacarlas. 
