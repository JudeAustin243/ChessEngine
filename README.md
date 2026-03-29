# Chess Engine

A high-performance chess engine written from scratch in C#, built on a bitboard-based architecture with fully legal move generation and an optimised alpha-beta search.

---

## Overview

This project is a complete chess engine implementation focused on correctness, performance, and extensibility. It supports all standard chess rules and has been rigorously validated using perft testing.

The engine can run in multiple modes, including interactive play, self-play, and analysis.

---

## Features

### Core Architecture

* Bitboard-based board representation
* Efficient move encoding and state management

### Move Generation

* Fully legal move generation (not pseudo-legal)
* Handles:

  * Pins and discovered checks
  * Check detection and legal move filtering
  * Castling (all legality rules enforced)
  * En passant
  * Promotions

### Search

* Alpha-beta pruning
* Quiescence search
* Transposition table
* Killer move heuristic

### Additional Functionality

* Opening book support
* Multiple engine modes:

  * Play mode (human vs engine)
  * Self-play
  * Analysis mode
  * Perft testing
  * Debugging tools

---

## Correctness

* Move generation validated using **perft up to depth 7**
* Correct handling of all standard chess edge cases

---

## Performance

* ~28–30 million nodes per second on a consumer laptop
* Efficient move generation and search ordering
* Designed for further optimisation and scaling

---

## Project Structure

```
MoveGeneration/   # Board representation and legal move generation
Search/           # Alpha-beta search and heuristics
Evaluation/       # Position evaluation logic
```

---

## Running the Engine

1. Open the project in Visual Studio or your preferred C# IDE
2. Build the solution
3. Run the main entry point

The engine supports multiple modes depending on configuration:

* Play mode
* Self-play
* Analysis
* Perft

---

## Example Capabilities

* Generate and validate move trees (perft)
* Play full games against a human
* Run automated self-play for testing
* Analyse positions using search

---

## Future Improvements

* Improved evaluation function
* Additional move ordering heuristics (e.g. history heuristic, MVV-LVA tuning)
* Iterative deepening and time management
* UCI protocol support
* Endgame tablebases

---

## Notes

This engine was built entirely from scratch with a focus on implementing the core components of modern chess engines, including efficient move generation and search optimisation techniques.
