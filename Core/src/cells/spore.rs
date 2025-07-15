use std::sync::{Arc, Mutex};
use crate::cells::{Cell, Action, Position};
use crate::data::Direction;
use rand::{Rng, SeedableRng};
use crate::cells::air::Air;
use crate::cells::dirt::Dirt;
use crate::cells::mycelium::Mycelium;
use crate::grid::grid;

pub struct Spore {
    ticks_on_dirt: Arc<Mutex<i32>>,
    ticks_on_air: Arc<Mutex<i32>>,
    air_ticks: Arc<Mutex<i32>>,
    direction: Arc<Mutex<Direction>>,
    rng: Arc<Mutex<rand::rngs::StdRng>>,
}

impl Spore {
    pub fn new() -> Self {
        let mut rng = rand::rngs::StdRng::from_os_rng();
        let direction = if rng.random_bool(0.5) {
            Direction::Left
        } else {
            Direction::Right
        };

        Self {
            ticks_on_dirt: Arc::new(Mutex::new(0)),
            ticks_on_air: Arc::new(Mutex::new(0)),
            air_ticks: Arc::new(Mutex::new(0)),
            direction: Arc::new(Mutex::new(direction)),
            rng: Arc::new(Mutex::new(rng)),
        }
    }
}

impl Cell for Spore {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut ticks_on_dirt = self.ticks_on_dirt.lock().unwrap();
        let mut ticks_on_air = self.ticks_on_air.lock().unwrap();
        let mut air_ticks = self.air_ticks.lock().unwrap();
        let mut direction = self.direction.lock().unwrap();

        let down_pos = position.get_neighbor(Direction::Down);
        if grid::get(down_pos).unwrap().as_any().is::<Dirt>() {
            *ticks_on_dirt += 1;

            if *ticks_on_dirt > 50 {
                return Some(Box::new(move || {
                    grid::set(
                        down_pos,
                        Arc::new(Mycelium::new(true, position)));
                    grid::set(position, Arc::new(Air));
                }));
            }
        } else {
            *ticks_on_air += 1;

            if *ticks_on_air > 200 {
                return Some(Box::new(move || {
                    grid::set(position, Arc::new(Air));
                }));
            }

            if grid::get(down_pos).unwrap().as_any().is::<Air>() {
                *air_ticks += 1;

                if *air_ticks >= 4 {
                    let left_down = Position::new(position.x - 1, position.y + 1);
                    let air_arc = self.air_ticks.clone();
                    return Some(Box::new(move || {
                        let mut air_ticks = air_arc.lock().unwrap();
                        grid::move_cell(position, left_down);
                        *air_ticks = 0;
                    }));
                }
            }
            
            let left_pos = Position::new(position.x - 1, position.y);
            let right_pos = Position::new(position.x + 1, position.y);

            let can_move_left = grid::get(left_pos).unwrap().as_any().is::<Air>();
            let can_move_right = grid::get(right_pos).unwrap().as_any().is::<Air>();

            if !can_move_left {
                *direction = Direction::Right;
            }
            if !can_move_right {
                *direction = Direction::Left;
            }

            if *direction == Direction::Left && can_move_left {
                return Some(Box::new(move || {
                    grid::move_cell(position, left_pos);
                }));
            } else if *direction == Direction::Right && can_move_right {
                return Some(Box::new(move || {
                    grid::move_cell(position, right_pos);
                }));
            }
        }

        None
    }

    fn get_color(&self, _: Position) -> String {
        "#f7f7f7".to_string()
    }

    fn clone_cell(&self) -> Arc<dyn Cell> {
        Arc::new(Self {
            ticks_on_dirt: Arc::clone(&self.ticks_on_dirt),
            ticks_on_air: Arc::clone(&self.ticks_on_air),
            air_ticks: Arc::clone(&self.air_ticks),
            direction: Arc::clone(&self.direction),
            rng: Arc::clone(&self.rng),
        })
    }
}