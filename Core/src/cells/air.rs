use crate::cells::{Action, Cell};
use crate::data::Position;

#[derive(Clone)]
pub struct Air;

impl Cell for Air {
    fn do_action(&self, _position: Position) -> Action {
        Box::new(|| {})
    }

    fn get_color(&self, _position: Position) -> String {
        String::from("#000000")
    }
}