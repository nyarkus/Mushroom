use std::sync::{Arc, Mutex};
use crate::cells::{Cell, Action, Position};
use crate::cells::air::Air;
use crate::cells::spore::Spore;
use crate::data::Direction;
use crate::grid::grid;

pub struct Cap {
    water: Arc<Mutex<f32>>,
    energy: Arc<Mutex<f32>>,
    dies: Arc<Mutex<i32>>,
    spore_ticks: Arc<Mutex<i32>>,
    stalk_position: Position,
    max_distance: i32,
}

impl Cap {
    pub fn new(stalk_position: Position, max_distance: i32) -> Self {
        Self {
            water: Arc::new(Mutex::new(1.0)),
            energy: Arc::new(Mutex::new(1.0)),
            dies: Arc::new(Mutex::new(0)),
            spore_ticks: Arc::new(Mutex::new(0)),
            stalk_position,
            max_distance,
        }
    }
}

impl Cell for Cap {
    fn do_action(&self, position: Position) -> Option<Action> {
        let mut water = self.water.lock().unwrap();
        let mut energy = self.energy.lock().unwrap();
        let mut dies = self.dies.lock().unwrap();
        let mut spore_ticks = self.spore_ticks.lock().unwrap();

        // Потребление ресурсов
        if *energy > 0.00005 { *energy -= 0.00005; } else { *dies += 1; }
        if *water > 0.0005 { *water -= 0.0005; } else { *dies += 1; }

        // Попытка восстановления
        if *dies > 0 && *energy > 0.2 && *water > 0.2 {
            *energy -= 0.1;
            *water -= 0.1;
            *dies -= 1;
        }

        // Смерть шляпки
        if *dies > 20 {
            return Some(Box::new(move || {
                grid::set(position, Arc::new(Air));
            }));
        }

        // Рост шляпки
        for dir in [Direction::Up, Direction::Left, Direction::Right] {
            let neighbor_pos = position.get_neighbor(dir);
            if position.distance_squared(neighbor_pos) < self.max_distance
                && grid::get(neighbor_pos).unwrap().as_any().is::<Air>()
                && *water > 0.2
                && *energy > 0.2
            {
                *water -= 0.1;
                *energy -= 0.1;

                let stalk_position = self.stalk_position;
                let max_distance = self.max_distance;
                return Some(Box::new(move || {
                    grid::set(
                        neighbor_pos,
                        Arc::new(Cap::new(stalk_position, max_distance))
                    );
                }));
            }
        }

        let down_pos = position.get_neighbor(Direction::Down);
        let left_pos = position.get_neighbor(Direction::Left);
        let right_pos = position.get_neighbor(Direction::Right);

        if grid::get(down_pos).unwrap().as_any().is::<Air>()
            && !grid::get(left_pos).unwrap().as_any().is::<Air>()
            && !grid::get(right_pos).unwrap().as_any().is::<Air>()
            && *water > 0.2
            && *energy > 0.2
        {
            *water -= 0.1;
            *energy -= 0.1;
            *spore_ticks += 1;

            if *spore_ticks >= 500 {
                let spore_ticks_clone = self.spore_ticks.clone();
                return Some(Box::new(move || {
                    let mut spore_ticks = spore_ticks_clone.lock().unwrap();
                    grid::set(
                        down_pos,
                        Arc::new(Spore::new()));
                    *spore_ticks = 0;
                }));
            }
        }

        None
    }

    fn get_color(&self, _: Position) -> String {
        "#4f3b1a".to_string()
    }

    fn clone_cell(&self) -> Arc<dyn Cell> {
        Arc::new(Self {
            water: Arc::clone(&self.water),
            energy: Arc::clone(&self.energy),
            dies: Arc::clone(&self.dies),
            spore_ticks: Arc::clone(&self.spore_ticks),
            stalk_position: self.stalk_position,
            max_distance: self.max_distance,
        })
    }
}