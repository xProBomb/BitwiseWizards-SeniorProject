// TrustTrade/jest.config.js
module.exports = {
    testEnvironment: 'jsdom',
    roots: ['<rootDir>/tests', '<rootDir>/wwwroot/js'],
    testMatch: ['**/*.test.js'],
    transform: {
        '^.+\\.js$': 'babel-jest',
    },
    moduleNameMapper: {
        // Map ~/js imports to the wwwroot/js directory
        '~/js/(.*)': '<rootDir>/wwwroot/js/$1'
    }
};