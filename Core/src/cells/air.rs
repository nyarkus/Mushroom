use crate::cells;
use crate::cells::Arc;
use crate::cells::{Action, Cell};
use crate::data::Position;

#[derive(Clone)]
pub struct Air;

impl Cell for Air {
    fn do_action(&self, position: Position) -> Option<Action> {
        Some(Box::new(|| {}))
    }

    fn get_color(&self, _position: Position) -> String {
        String::from("#000000")
    }
    fn clone_cell(&self) -> Arc<(dyn cells::Cell + 'static)> {
        let new_dirt = Air {
        };
        Arc::new(new_dirt)
    }
}