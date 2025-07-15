use easy_color::{Hex, IntoRGB, RGB};
use crate::cells::dirt::Dirt;
use crate::cells::air::Air;
use strum::IntoEnumIterator;
use crate::cells;
use crate::cells::{Action, Cell, AsAny};
use crate::data::{Direction, Position};
use crate::grid::grid;
use std::sync::{Arc, Mutex};
use rand::rngs::StdRng;
use rand::{Rng, SeedableRng};

#[derive(Clone)]
pub struct Water {
    pub life_time: Arc<Mutex<i32>>,
    rnd: Arc<Mutex<StdRng>>
}

impl Water {
    pub fn new() -> Self {
        Self {
            life_time: Arc::new(Mutex::new(0)),
            rnd: Arc::new(Mutex::new(StdRng::from_os_rng()))
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
                    let cloned_neighbor = neighbor.clone();
                    if let Some(target_dirt) = neighbor.as_any().downcast_ref::<Dirt>() {
                        let target_dampness = target_dirt.dampness.lock().unwrap();
                        if target_dampness.clone() < 1.0 {
                            return Some(Box::new(move || {
                                if let Some(dirt) = cloned_neighbor.as_any().downcast_ref::<Dirt>() {
                                    let mut dampness = dirt.dampness.lock().unwrap();

                                    let value = dampness.clone();
                                    *dampness = (value + 0.3);
                                }
                            }));
                        }
                    }
                }
            }
        }

        let mut life_time = self.life_time.lock().unwrap();
        if isAir && waterCells < 3 {
            *life_time = *life_time + 1;
        }

        if *life_time > 300 {
            return Some(Box::new(move || {
                grid::set(position, Arc::new(Air { }))
            }))
        }

        let down_position = Position::new(position.x, position.y + 1);
        if grid::get(down_position).as_any().is::<Air>() {
            return Some(Box::new(move || {
                grid::move_cell(position, down_position)
            }))
        }

        let down_left_position = Position::new(position.x - 1, position.y + 1);
        let down_right_position = Position::new(position.x + 1, position.y + 1);

        let can_flow_left_down = grid::get(down_left_position).as_any().is::<Air>();
        let can_flow_right_down = grid::get(down_right_position).as_any().is::<Air>();

        let mut rand = self.rnd.lock().unwrap();
        if can_flow_left_down && can_flow_right_down {
            let target_position = if rand.random_range(0..=1) == 0 { down_left_position } else { down_right_position };
            return Some(Box::new(move || {
                grid::move_cell(position, target_position)
            }))
        }
        if can_flow_left_down {
            return Some(Box::new(move || {
                grid::move_cell(position, down_left_position)
            }))
        }
        if can_flow_right_down {
            return Some(Box::new(move || {
                grid::move_cell(position, down_right_position)
            }))
        }

        Some(Box::new(move || {}))
    }

    fn get_color(&self, _position: Position) -> String {
        let mut depth = 0;
        let mut currentPos = Position::new(_position.x, _position.y + 1);

        while grid::get(currentPos).as_any().is::<Water>() && depth < 10 {
            depth += 1;
            currentPos = Position::new(currentPos.x, currentPos.y + 1);
        }
        let baseColor: Hex = "#55a8e8".try_into().unwrap();
        let mut rgb: RGB = baseColor.into();

        rgb.set_red((rgb.red() - depth * 15).max(0));
        rgb.set_green((rgb.green() - depth * 15).max(0));
        rgb.set_blue((rgb.blue() - depth * 10).max(0));

        Hex::from(rgb).to_string()
    }
    fn clone_cell(&self) -> Arc<(dyn cells::Cell + 'static)> {
        let new_water = Water {
            life_time: Arc::new(Mutex::new(*self.life_time.lock().unwrap())),
            rnd: self.rnd.clone()
        };
        Arc::new(new_water)
    }
}