# TODO

## Quit releases services before scene systems

On quit, and when play mode stops in the editor, every `InitializablesContainer` releases itself
from its `OnApplicationQuitEvent` handler. Handlers run in subscription order, so the services
container (installed by the Entry scene) releases before the core systems container of the open
scene. A core system or module that calls a service from its own release then finds the service
torn down: `ResourcesService.OnRelease` clears its dictionary, so `StopTicking(type)` called from a
core system's release throws `KeyNotFoundException`.

Found in PopDrop, where `SpeedUpBooster` stopped its ticking resource on dispose. It now holds the
`ResourceRuntimeData` it resolved on initialize, which keeps working after the service's release.

Fix direction: release scene containers before services on quit, in reverse install order. Needs
testing: services that save on release must still run, and `CoreSceneStarter.OnDestroy` must
stay harmless after the quit release has already emptied the container.
