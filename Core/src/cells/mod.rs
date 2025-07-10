pub mod air;
mod dirt;

use crate::data::Position;

pub type Action = Box<dyn FnOnce()>;

pub trait Cell: DynClone + Send + Sync {
    fn do_action(&self, position: Position) -> Action;
    fn get_color(&self, position: Position) -> String;
}

pub trait DynClone {
    fn clone_box(&self) -> Box<dyn Cell>;
}

impl<T> DynClone for T
where
    T: 'static + Cell + Clone,
{
    fn clone_box(&self) -> Box<dyn Cell> {
        Box::new(self.clone())
    }
}

impl Clone for Box<dyn Cell> {
    fn clone(&self) -> Box<dyn Cell> {
        self.clone_box()
    }
}