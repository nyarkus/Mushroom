use crate::cells::air::Air;
use crate::cells::water::Water;
use crate::grid::grid;
use std::sync::Mutex;
use strum::IntoEnumIterator;
use crate::cells;
use crate::cells::{Arc, AsAny};
use crate::cells::{Action, Cell};
use crate::data::{Direction, Position};

#[derive(Clone)]
pub struct Sand {
    pub is_wet: Arc<Mutex<bool>>,
    ticks_in_air: Arc<Mutex<i32>>
}

impl Cell for Sand {
    fn do_action(&self, position: Position) -> Option<Action> {
        let down = grid::get_neighbor(position, Direction::Down);

        if down.as_any().is::<Air>() {
            let down_position = Position::new(position.x, position.y + 1);
            return Some(Box::new(move || {
                grid::move_cell(position, down_position);
            }))
        }

        for dir in Direction::iter() {
            let neighbor = grid::get_neighbor(position, dir);
            if neighbor.as_any().is::<Water>() {
                *self.is_wet.lock().unwrap() = true;
            }
            if neighbor.as_any().is::<Air>() {
                let ticks = *self.ticks_in_air.lock().unwrap();
                *self.ticks_in_air.lock().unwrap() = ticks + 1;
            }
        }

        if *self.ticks_in_air.lock().unwrap() > 10 {
            *self.is_wet.lock().unwrap() = false;
            *self.ticks_in_air.lock().unwrap() = 0;
        }

        Some(Box::new(move || {}))
    }

    fn get_color(&self, _position: Position) -> String {
        if *self.is_wet.lock().unwrap() { String::from("#abad3a") } else { String::from("#dde04c") }
    }
    fn clone_cell(&self) -> Arc<(dyn cells::Cell + 'static)> {
        let new_sand = Sand {
            is_wet: Arc::new(Mutex::new(*self.is_wet.lock().unwrap())),
            ticks_in_air: Arc::new(Mutex::new(*self.ticks_in_air.lock().unwrap()))
        };
        Arc::new(new_sand)
    }
}