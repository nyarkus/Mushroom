use easy_color::{Hex, IntoRGB};
use strum_macros::EnumIter;
#[derive(Debug, Copy, Clone, PartialEq, Eq, Hash)]
pub struct Position {
    pub x: i32,
    pub y: i32,
}

impl Position {
    pub fn new(x: i32, y: i32) -> Self {
        Self { x, y }
    }
    pub fn distance_squared(&self, other: Position) -> i32 {
        let dx = self.x - other.x;
        let dy = self.y - other.y;

        dx * dx + dy * dy
    }

    pub fn get_neighbor(&self, dir: Direction) -> Position {
        match dir {
            Direction::Up => Position::new(self.x, self.y - 1),
            Direction::Down => Position::new(self.x, self.y + 1),
            Direction::Left => Position::new(self.x - 1, self.y),
            Direction::Right => Position::new(self.x + 1, self.y),
        }
    }
}

#[derive(Debug, Copy, Clone)]
pub struct Size {
    pub x: i32,
    pub y: i32,
}

impl Size {
    pub fn new(x: i32, y: i32) -> Size {
        Size { x, y }
    }
}

#[derive(Debug, Copy, Clone, EnumIter)]
#[derive(PartialEq)]
pub enum Direction {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

impl Direction {
    pub fn from(value: i32) -> Direction {
        match value {
            0 => Direction::Up,
            1 => Direction::Down,
            2 => Direction::Left,
            3 => Direction::Right,
            _ => panic!("Invalid direction: {}", value),
        }
    }
}

#[repr(i32)]
pub enum Cell {
    Air = 0,
    Dirt = 1,
    Water = 2,
    Sand = 3,
    Mycelium = 4,
    Stalk = 5,
    Cap = 6,
    Spore = 7,
}

#[repr(C)]
pub struct CellRenderData {
    pub r: u8,
    pub g: u8,
    pub b: u8,
}

impl CellRenderData {
    pub fn new(hex: String) -> Self {
        let str = hex.as_str();
        let hex: Hex = str.try_into().unwrap();
        let rgb = hex.to_rgb();

        CellRenderData {
            r: rgb.red(),
            g: rgb.green(),
            b: rgb.blue(),
        }

    }
}