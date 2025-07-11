use crate::cells::dirt::Dirt;
use crate::cells::air::Air;
use strum::IntoEnumIterator;
use crate::cells;
use crate::cells::{Action, Cell, AsAny};
use crate::data::{Direction, Position};
use crate::grid::grid;
use std::sync::{Arc, Mutex};

#[derive(Clone)]
pub struct Water {
    pub life_time: Arc<Mutex<i32>>,
}

impl Water {
    pub fn new() -> Self {
        Self {
            life_time: Arc::new(Mutex::new(0)),
        }
    }
}

impl Cell for Water {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut waterCells = 0;
        let mut isAir = false;

        for dir in Direction::iter() {

            if let Some(neighbor) = grid::get_neighbor(position, dir) {
                if neighbor.as_any().is::<Water>() {
                    waterCells += 1;
                }
                else if neighbor.as_any().is::<Air>() {
                    isAir = true;
                }
                else if neighbor.as_any().is::<Dirt>() {
                    if let Some(target_dirt) = neighbor.as_any().downcast_ref::<Dirt>() {
                        let mut target_dampness = target_dirt.dampness.lock().unwrap();
                        return Some(Box::new(move || {
   
                            let value = dampness.clone();
                            *dampness = (value + 0.3);
                        }));
                    }
                }
            }
        }

        Some(Box::new(move || {}))
    }

    fn get_color(&self, _position: Position) -> String {
        String::from("#000000")
    }
    fn clone_cell(&self) -> Arc<(dyn cells::Cell + 'static)> {
        let new_water = Water {
        };
        Arc::new(new_water)
    }
}