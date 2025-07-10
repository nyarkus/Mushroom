pub mod grid {
    use std::collections::HashMap;
    use std::sync::Mutex;
    use lazy_static::lazy_static;
    use crate::cells::Cell;
    use crate::data::*;

    struct WorldState {
        cells: HashMap<(i32, i32), Box<dyn Cell>>,
        size: Size,
        ground_level: i32,
    }

    lazy_static! {
                static ref WORLD: Mutex<WorldState> = Mutex::new(WorldState {
                    cells: HashMap::new(),
                    size: Size { x: 0, y: 0 },
                    ground_level: 0,
                });
            }

    pub fn set(position: Position, cell: Box<dyn Cell>) {
        let mut world = WORLD.lock().unwrap();
        world.cells.insert((position.x, position.y), cell);
    }

    pub fn get(position: Position) -> Box<dyn Cell> {
        let world = WORLD.lock().unwrap();
        world.cells
            .get(&(position.x, position.y))
            .cloned()
            .unwrap_or_else(|| Box::new(crate::cells::air::Air))
    }

    pub fn move_cell(from: Position, to: Position) {
        if from == to { return; }

        let mut world = WORLD.lock().unwrap();

        if let Some(cell_from) = world.cells.remove(&(from.x, from.y)) {
            if let Some(cell_to) = world.cells.remove(&(to.x, to.y)) {
                world.cells.insert((from.x, from.y), cell_to);
            }
            world.cells.insert((to.x, to.y), cell_from);
        }
    }

    pub fn get_neighbor(position: Position, direction: Direction) -> Box<dyn Cell> {
        let target_pos = match direction {
            Direction::Up => Position::new(position.x, position.y - 1),
            Direction::Down => Position::new(position.x, position.y + 1),
            Direction::Left => Position::new(position.x - 1, position.y),
            Direction::Right => Position::new(position.x + 1, position.y),
        };
        get(target_pos)
    }

    pub fn is_in_bounds(position: Position) -> bool {
        let world = WORLD.lock().unwrap();
        position.x >= 0 && position.x < world.size.x && position.y >= 0 && position.y < world.size.y
    }

    pub fn get_size() -> Size {
        WORLD.lock().unwrap().size
    }

    pub fn set_size(new_size: Size) {
        WORLD.lock().unwrap().size = new_size;
    }

    pub fn get_ground_level() -> i32 {
        WORLD.lock().unwrap().ground_level
    }

    pub fn set_ground_level(level: i32) {
        WORLD.lock().unwrap().ground_level = level;
    }
}