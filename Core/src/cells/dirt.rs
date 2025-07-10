use crate::cells::{Action, Cell};
use crate::data::Position;

#[derive(Clone)]
pub struct Dirt;

impl Cell for Dirt {
    fn do_action(&self, _position: Position) -> Action {
        Box::new(|| {})
    }

    fn get_color(&self, _position: Position) -> String {
        String::from("#000000")
    }
}