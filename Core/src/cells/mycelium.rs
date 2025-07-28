use std::sync::{Arc, Mutex};
use crate::cells::{Cell, Action};
use crate::data::{Direction, Position};
use rand::{Rng, SeedableRng};
use rand::rngs::StdRng;
use strum::IntoEnumIterator;
use crate::cells::air::Air;
use crate::cells::dirt::Dirt;
use crate::cells::stalk::Stalk;
use crate::grid::grid;

pub struct Mycelium {
    pub water: Arc<Mutex<f32>>,
    pub energy: Arc<Mutex<f32>>,
    main: bool,
    dies: Arc<Mutex<u32>>,
    main_position: Position,
    max_distance: i32,
    rng: Arc<Mutex<StdRng>>,
}

impl Mycelium {
    pub fn new(main: bool, main_position: Position) -> Self {
        let rng = StdRng::from_os_rng();
        let max_distance = Position::new(1, 1).distance_squared(Position::new(5, 5));

        Self {
            water: Arc::new(Mutex::new(if main { 100.0 } else { 0.2 })),
            energy: Arc::new(Mutex::new(if main { 100.0 } else { 0.2 })),
            main,
            dies: Arc::new(Mutex::new(0)),
            main_position,
            max_distance,
            rng: Arc::new(Mutex::new(rng)),
        }
    }
}

impl Cell for Mycelium {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut water = self.water.lock().unwrap();
        let mut energy = self.energy.lock().unwrap();
        let mut dies = self.dies.lock().unwrap();
        let mut rng = self.rng.lock().unwrap();

        if self.main {
            if *energy > 0.42 && *water > 0.47 {
                let down = grid::get_neighbor(position, Direction::Down).unwrap();
                if down.as_any().is::<Air>() {
                    *energy -= 0.42;
                    *water -= 0.47;

                    return Some(Box::new(move || {
                        grid::set(
                            Position::new(position.x, position.y + 1),
                            Arc::new(Mycelium::new(false, position))
                        );
                    }));
                }
            }
            
            if *energy > 0.8 && *water > 0.85 {
                let up = grid::get_neighbor(position, Direction::Up).unwrap();
                if up.as_any().is::<Air>() {
                    let target_y = grid::get_ground_level() - rng.random_range(2..6);
                    *water -= 0.65;
                    *energy -= 0.35;

                    return Some(Box::new(move || {
                        grid::set(
                            Position::new(position.x, position.y - 1),
                            Arc::new(Stalk::new(target_y))
                        );
                    }));
                }
            }
        }

        if *energy > 0.0 { *energy -= 0.0005; } else { *dies += 1; }
        if *water > 0.0 { *water -= 0.0008; } else { *dies += 1; }

        if *dies > 0 && *energy > 0.15 && *water > 0.25 {
            *energy -= 0.15;
            *water -= 0.25;
            *dies -= 1;
        }

        if *dies >= 100 {
            return Some(Box::new(move || {
                grid::set(
                    position,
                    Arc::new(Dirt { dampness: Arc::new(Mutex::new(0.0)), nutrients: Arc::new(Mutex::new(0.5)) })
                );
            }));
        }

        for dir in Direction::iter() {
            let neighbor = grid::get_neighbor(position, dir).unwrap();
            if let Some(cell) = neighbor.as_any().downcast_ref::<Dirt>() {
                let mut dampness = cell.dampness.lock().unwrap();
                let mut nutrients = cell.nutrients.lock().unwrap();

                if *dampness >= 0.1 {
                    *dampness -= 0.1;
                    *water += 0.1;
                }
                if *nutrients >= 0.1 {
                    *nutrients -= 0.1;
                    *energy += 0.05;
                }
            }
        }

        if !self.main && position.distance_squared(self.main_position) < self.max_distance {
            if *energy > 0.85 && *water > 0.95 {
                let dir = Direction::from(rng.random_range(0..=3));
                let neighbor_pos = position.get_neighbor(dir);

                if grid::get(neighbor_pos).unwrap().as_any().is::<Dirt>() {
                    *energy -= 0.75;
                    *water -= 0.85;

                    let main_position = self.main_position;
                    return Some(Box::new(move || {
                        grid::set(
                            neighbor_pos,
                            Arc::new(Mycelium::new(false, main_position))
                        );
                    }));
                }
            }
        }

        None
    }

    fn get_color(&self, _: Position) -> String {
        let energy = *self.energy.lock().unwrap();
        match energy {
            _ if energy == 0.0 => "#636363",
            _ if energy < 0.3 => "#8e8e8e",
            _ if energy < 0.6 => "#b7b7b7",
            _ => "#cecece",
        }.to_string()
    }

    fn clone_cell(&self) -> Arc<dyn Cell> {
        Arc::new(Self {
            water: Arc::clone(&self.water),
            energy: Arc::clone(&self.energy),
            main: self.main,
            dies: Arc::clone(&self.dies),
            main_position: self.main_position,
            max_distance: self.max_distance,
            rng: Arc::clone(&self.rng),
        })
    }
}