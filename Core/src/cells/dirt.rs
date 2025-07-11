use crate::cells;
use crate::cells::{Action, Cell, AsAny};
use crate::data::{Direction, Position};
use crate::grid::grid;
use crate::cells::air::Air;
use std::sync::{Arc, Mutex};

const MAX_DAMPNESS: f32 = 1.0;
const MIN_DAMPNESS: f32 = 0.0;
const EVAPORATION_RATE: f32 = 0.005;
const GRAVITY_FLOW_RATE: f32 = 0.2;
const HORIZONTAL_FLOW_RATE: f32 = 0.1;

#[derive(Clone)]
pub struct Dirt {
    pub dampness: Arc<Mutex<f32>>,
    pub nutrients: Arc<Mutex<f32>>,
}

impl Dirt {
    pub fn new() -> Self {
        Self {
            dampness: Arc::new(Mutex::new(0.9)),
            nutrients: Arc::new(Mutex::new(1.0)),
        }
    }
}

impl Cell for Dirt {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut pending_changes: Vec<(Arc<dyn Cell>, f32)> = Vec::new();
        let mut current_dampness_change: f32 = 0.0;

        let initial_dampness = *self.dampness.lock().unwrap();

        if let Some(up) = grid::get_neighbor(position, Direction::Up) {
            if up.as_any().is::<Air>() && initial_dampness > MIN_DAMPNESS {
                current_dampness_change -= EVAPORATION_RATE;
            }
        }

        if let Some(down) = grid::get_neighbor(position, Direction::Down) {
            if let Some(dirt_down) = down.as_any().downcast_ref::<Dirt>() {
                let neighbor_dampness = *dirt_down.dampness.lock().unwrap();
                if initial_dampness > neighbor_dampness {
                    let amount_to_flow = (initial_dampness - neighbor_dampness) / 2.0 * GRAVITY_FLOW_RATE;
                    current_dampness_change -= amount_to_flow;
                    pending_changes.push((down.clone(), amount_to_flow));
                }
            }
        }

        if let Some(right) = grid::get_neighbor(position, Direction::Right) {
            if let Some(dirt_right) = right.as_any().downcast_ref::<Dirt>() {
                let neighbor_dampness = *dirt_right.dampness.lock().unwrap();
                if initial_dampness > neighbor_dampness {
                    let amount_to_flow = (initial_dampness - neighbor_dampness) / 2.0 * HORIZONTAL_FLOW_RATE;
                    current_dampness_change -= amount_to_flow;
                    pending_changes.push((right.clone(), amount_to_flow));
                }
            }
        }

        if let Some(left) = grid::get_neighbor(position, Direction::Left) {
            if let Some(dirt_left) = left.as_any().downcast_ref::<Dirt>() {
                let neighbor_dampness = *dirt_left.dampness.lock().unwrap();
                if initial_dampness < neighbor_dampness {
                    let amount_to_flow = (neighbor_dampness - initial_dampness) / 2.0 * HORIZONTAL_FLOW_RATE;
                    current_dampness_change += amount_to_flow;
                }
            }
        }

        if current_dampness_change.abs() > 0.0001 || !pending_changes.is_empty() {
            let self_dampness_arc = self.dampness.clone();

            return Some(Box::new(move || {
                let mut dampness = self_dampness_arc.lock().unwrap();
                *dampness += current_dampness_change;

                for (cell, dampness_delta) in pending_changes {
                    if let Some(target_dirt) = cell.as_any().downcast_ref::<Dirt>() {
                        let mut target_dampness = target_dirt.dampness.lock().unwrap();
                        *target_dampness += dampness_delta;
                    }
                }

                *dampness = dampness.clamp(MIN_DAMPNESS, MAX_DAMPNESS);
            }));
        }

        None
    }

    fn get_color(&self, _position: Position) -> String {
        let dry_color = (0xA0, 0x82, 0x61);
        let wet_color = (0x38, 0x25, 0x11);

        let t = (*self.dampness.lock().unwrap() / MAX_DAMPNESS).clamp(0.0, 1.0);

        let r = (dry_color.0 as f32 + (wet_color.0 as f32 - dry_color.0 as f32) * t) as u8;
        let g = (dry_color.1 as f32 + (wet_color.1 as f32 - dry_color.1 as f32) * t) as u8;
        let b = (dry_color.2 as f32 + (wet_color.2 as f32 - dry_color.2 as f32) * t) as u8;

        format!("#{:02X}{:02X}{:02X}", r, g, b)
    }
    fn clone_cell(&self) -> Arc<(dyn cells::Cell + 'static)> {
        let new_dirt = Dirt {
            dampness: Arc::new(Mutex::new(*self.dampness.lock().unwrap())),
            nutrients: Arc::new(Mutex::new(*self.nutrients.lock().unwrap())),
        };
        Arc::new(new_dirt)
    }
}