# Projet DNA
### Rémi Plantade - Alexandre Schwarze - Bastien Penetro

## Architecture de l'application 
![Diagramme de classe](https://github.com/lowxorx/c-projet-adn/blob/master/NodeNet/Doc/dgc.png)
![Diagramme de composants](https://github.com/lowxorx/c-projet-adn/blob/master/NodeNet/Doc/dgcompo.png)

## Fonctionnalités infrastructure
* Gestion d’un cluster de nœuds
* Monitoring des nœuds / tasks (UC, méthodes)
* Load balancing & lazy loading
* Traçabilité des évènements (UC, méthodes)
* Gestion multi-clients
* Multihreading

## Fonctionnalités métier
* Comptage des bases simples & inconnues, pourcentage
* Comptage des paires de bases 
* Détection de la séquence de 4 bases la plus fréquente
* Conservation de l’intégrité du génome
* Fiabilité des résultats

## Implémentation de l'infrasctructure
### Classes à implémenter:

* DNAClient => DefaultClient
* DNANode => DefaultNode
* DNAOrchestra => Orchestrator
* Mapper / Reducer

* Ajout des TaskExecutor à la Factory avec les méthodes appropriées

* Envoi du mapper / reducer par le biais du TaskExecutor

* Gestion au niveau infra du monitoring, utilisation des UC

## Aperçu de l'IHM 
![Diagramme de classe](https://github.com/lowxorx/c-projet-adn/blob/master/NodeNet/Doc/ihm.png)
