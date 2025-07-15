use std::sync::{Arc, Mutex};
use crate::cells::{Cell, Action, Position};
use crate::data::Direction;
use rand::{Rng, SeedableRng};
use crate::cells::air::Air;
use crate::cells::cap::Cap;
use crate::cells::mycelium::Mycelium;
use crate::grid::grid;

pub struct Stalk {
    water: Arc<Mutex<f32>>,
    energy: Arc<Mutex<f32>>,
    dies: Arc<Mutex<u32>>,
    target_y: i32,
    rng: Arc<Mutex<rand::rngs::StdRng>>,
}

impl Stalk {
    pub fn new(target_y: i32) -> Self {
        Self {
            water: Arc::new(Mutex::new(0.6)),
            energy: Arc::new(Mutex::new(0.3)),
            dies: Arc::new(Mutex::new(0)),
            target_y,
            rng: Arc::new(Mutex::new(rand::rngs::StdRng::from_os_rng())),
        }
    }
}

impl Cell for Stalk {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut water = self.water.lock().unwrap();
        let mut energy = self.energy.lock().unwrap();
        let mut dies = self.dies.lock().unwrap();
        let mut rng = self.rng.lock().unwrap();

        if *energy > 0.0001 { *energy -= 0.0001; } else { *dies += 1; }
        if *water > 0.001 { *water -= 0.001; } else { *dies += 1; }

        if *dies > 0 && *energy > 0.2 && *water > 0.2 {
            *energy -= 0.1;
            *water -= 0.1;
            *dies -= 1;
        }

        if *dies > 50 {
            return Some(Box::new(move || {
                let down_pos = position.get_neighbor(Direction::Down);
                if let Some(cell) = grid::get(down_pos).unwrap().as_any().downcast_ref::<Mycelium>() {
                    *cell.water.lock().unwrap() += 0.1;
                    *cell.energy.lock().unwrap() += 0.1;
                }
                grid::set(position, Arc::new(Air));
            }));
        }

        if position.y > self.target_y && *water >= 0.6 && *energy >= 0.3 {
            let up_pos = position.get_neighbor(Direction::Up);
            if grid::get(up_pos).unwrap().as_any().is::<Air>() {
                *water -= 0.6;
                *energy -= 0.3;

                let target_y = self.target_y;
                return Some(Box::new(move || {
                    grid::set(
                        up_pos,
                        Arc::new(Stalk::new(target_y))
                    );
                }));
            }
        }

        if position.y == self.target_y {
            if *energy > 0.5 && *water > 0.5 {
                let up_pos = position.get_neighbor(Direction::Up);
                if grid::get(up_pos).unwrap().as_any().is::<Air>() {
                    let cap_size = rng.random_range(3..6);
                    let max_dist = Position::new(1, 1).distance_squared(Position::new(cap_size, cap_size));

                    *water -= 0.4;
                    *energy -= 0.4;

                    return Some(Box::new(move || {
                        grid::set(
                            up_pos,
                            Arc::new(Cap::new(position, max_dist))
                        );
                    }));
                }
            }
        }

        None
    }

    fn get_color(&self, _: Position) -> String {
        "#f7f8f9".to_string()
    }

    fn clone_cell(&self) -> Arc<dyn Cell> {
        Arc::new(Self {
            water: Arc::clone(&self.water),
            energy: Arc::clone(&self.energy),
            dies: Arc::clone(&self.dies),
            target_y: self.target_y,
            rng: Arc::clone(&self.rng),
        })
    }
}