pub mod air;
pub mod dirt;
pub mod water;

use crate::data::Position;
use std::any::Any;
use std::sync::Arc;

pub type Action = Box<dyn FnOnce() + Send + Sync>;

pub trait AsAny: 'static {
    fn as_any(&self) -> &dyn Any;
}

impl<T: 'static> AsAny for T {
    fn as_any(&self) -> &dyn Any {
        self
    }
}

pub trait Cell: Send + Sync + AsAny {
    fn do_action(&self, position: Position) -> Option<Action>;
    fn get_color(&self, position: Position) -> String;
    fn clone_cell(&self) -> Arc<dyn Cell>;
}